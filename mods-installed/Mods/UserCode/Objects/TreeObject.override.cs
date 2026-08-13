// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.
namespace Eco.Mods.Organisms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Eco.Core.Utils;
    using Eco.Core.Items;
    using Eco.Gameplay.Interactions;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Minimap;
    using Eco.Gameplay.Plants;
    using Eco.Gameplay.Players;
    using Eco.Gameplay.Rooms;
    using Eco.Gameplay.Systems.TextLinks;
    using Eco.Shared;
    using Eco.Shared.Math;
    using Eco.Shared.Networking;
    using Eco.Shared.Serialization;
    using Eco.Shared.Utils;
    using Eco.Shared.SharedTypes;
    using Eco.Simulation;
    using Eco.Simulation.Agents;
    using Eco.Simulation.Types;
    using Eco.World;
    using Eco.World.Blocks;
    using Eco.Gameplay.Bonuses;
    using Eco.Gameplay.GameActions;
    using Eco.Mods.TechTree;
    using Eco.Simulation.WorldLayers.Pushers;
    using Eco.Shared.Localization;
    using Eco.Shared.Items;
    using Eco.Gameplay.Civics;
    using Vector3 = System.Numerics.Vector3;
    using System.ComponentModel;
    using Eco.Gameplay.Components;
    using Eco.Gameplay.Components.Storage;
    using Eco.Gameplay.Interactions.Interactors;
    using Eco.Gameplay.Objects;
    using Eco.Shared.Time;

    [Serialized]
    [Tag(BlockTags.Choppable)]
    class TrunkPiece
    {
        [Serialized] public Guid ID;
        [Serialized] public float SliceStart;
        [Serialized] public float SliceEnd;

        public double LastUpdateTime;
        [Serialized] public Vector3 Position;
        [Serialized] public Vector3 Velocity;
        [Serialized] public Quaternion Rotation;

        [Serialized] public bool Collected;

        public bool IsValid => World.IsLegalVerticalPosition(this.Position);
        public bool IsCollectedOrNotValid => !this.IsValid || this.Collected; // Returns collected if valid. We will count this as collected if its not valid

        public BSONObject ToUpdateBson()
        {
            var bson    = BSONObject.New;
            bson["id"]  = this.ID;
            bson["pos"] = this.Position;
            bson["rot"] = this.Rotation;
            bson["v"]   = this.Velocity;
            return bson;
        }

        public BSONObject ToInitialBson()
        {
            var bson          = BSONObject.New;
            bson["id"]        = this.ID;
            bson["start"]     = this.SliceStart;
            bson["end"]       = this.SliceEnd;
            bson["pos"]       = this.Position;
            bson["rot"]       = this.Rotation;
            bson["v"]         = this.Velocity;
            bson["collected"] = this.Collected;
            return bson;
        }
    }

    // gameplay version of simulations tree
    [Serialized] public partial class TreeEntity : Tree, IHasInteractions
    {
        readonly object sync = new();

        /// <summary>This needs to be 5, because 5 is the max yield bonus, and 5+5=10 is the max log stack size.</summary>
        private const int MaxTrunkPickupSize = 5;
        private static readonly LocString LogTooLargeMessage = Localizer.DoStr("Log is too large to pick up, slice into smaller pieces first.");
        /// <summary>Max number of tree debris spawn from the tree.</summary>
        private const int MaxTreeDebris = 20;

        // the list of all the slices done to this trunk
        [Serialized] readonly ThreadSafeList<TrunkPiece> trunkPieces = new ThreadSafeList<TrunkPiece>();

        public override bool UpRooted => this.stumpHealth <= 0;

        public override float SaplingGrowthPercent => 0.3f;

        // estimated, need to get better measurements w/ and w/o Top branch
        private static readonly float[] GrowthThresholds = new float[7] { 0.20f, 0.28f, 0.38f, 0.48f, 0.57f, 0.78f, 0.95f };
        private int currentGrowthThreshold = 0;
        private int treeDebrisSpawned; // it is runtime variable, it shouldn't spawn from felt trees so should be fine, but even if hacked someway it will produce at max 10 debris after restart

        public double LastUpdateTime { get; private set; }
        float lastKeyframeTime;

        [Serialized] public int ChopperUserID { get; protected set; } = -1;//who chopped down this tree have access to its trunks regardless of thier position

        public override IEnumerable<Vector3> TrunkPositions { get { return this.trunkPieces.Where(x => !x.Collected).Select(x => x.Position); } }

        private ThreadSafeHashSet<Vector3i> groundHits;
        MinimapObject minimapObject; //Allocated by Initialize via the bulk constructor.
        Vector3 baseScale;           //Species/random-derived scale; constant for the lifetime of the tree.
        bool baseScaleComputed;      //Sentinel so a legitimate (0,0,0) baseScale does not force recomputation every call.

        public override bool Ripe
        {
            get
            {
                if (this.Fallen && this.trunkPieces.All(piece => piece.IsCollectedOrNotValid))
                    return false;
                return this.GrowthPercent >= this.SaplingGrowthPercent;
            }
        }

        public override bool GrowthBlocked
        {
            get
            {
                if (this.Fallen || this.GrowthPercent >= 1f)
                    return true;
                if ((this.currentGrowthThreshold >= GrowthThresholds.Length) // already at max occupied spaces
                    || this.GrowthPercent < GrowthThresholds[this.currentGrowthThreshold])
                    return false;
                var block = World.GetBlock(this.Position.XYZi() + (Vector3i.Up * (this.currentGrowthThreshold + 1)));
                if (!block.Is<Empty>() && block.GetType() != this.Species.BlockType)
                        return true; // can't grow until obstruction is removed
                return false;
            }
        }

        public override float GrowthPercent
        {
            get => base.GrowthPercent;
            set { base.GrowthPercent = Mathf.Clamp01(value); this.UpdateGrowthOccupancy(); }
        }

        private void UpdateGrowthOccupancy()
        {
            if (this.IsGrowthConditionsNotMet) return; //Check if we can't grow, species is null or is already fallen.

            WrappedWorldPosition3i position = this.Position.XYZi() + (Vector3i.Up * this.currentGrowthThreshold);
            bool canGrow = position.TryIncreaseY(1, out position);

            if (canGrow)
            {
                // check if block is empty
                var block = World.GetBlock(position);
                if (!block.Is<Empty>() && block.GetType() != this.Species.BlockType) canGrow = false;
            }

            if (!canGrow)
            {
                base.GrowthPercent = GrowthThresholds[this.currentGrowthThreshold] - 0.01f;
                return; // cap growth at slightly less than threshold, can't grow until obstruction is removed
            }

            this.currentGrowthThreshold++;
            World.SetBlock(this.Species.BlockType, position);
            this.UpdateMinimapObjectScale(); //Update minimap tree icon only when growth threshold is reached.
            this.LastUpdateTime = TimeUtil.Seconds;
        }

        private bool IsGrowthConditionsNotMet => this.Species == null || this.Fallen || this.currentGrowthThreshold >= GrowthThresholds.Length || this.GrowthPercent < GrowthThresholds[this.currentGrowthThreshold];
        private bool CanHarvest => this.Fallen;

        public INetObjectViewer Controller { get; private set; }
    
        float ResourceMultiplier => (this.Species.ResourceRange.Diff * this.GrowthPercent) + this.Species.ResourceRange.Min;
        int GetBasePickupSize(TrunkPiece trunk) => Math.Max(Mathf.RoundPositivelyInt((trunk.SliceEnd - trunk.SliceStart) * this.ResourceMultiplier), 1);

        [Interaction(InteractionTrigger.InteractKey, requiredEnvVars: new[] { "id" }, animationDriven: true)]                        //A definition for when we can actually pickup
        public void PickUp(Player player, InteractionTriggerInfo trigger, InteractionTarget target) { if (target.TryGetParameter("id", out var id)) this.PickupLog(player, (Guid) id, target.HitPos); }


        #region IController
        int controllerID;

        public ref int ControllerID => ref this.controllerID;
    #pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged; //Disabled warning for property not being used, but its needed for change notifications to work
    #pragma warning restore CS0067
        #endregion

        public TreeEntity(TreeSpecies species, WorldPosition3i position, PlantPack plantPack)
            : base(species, position, plantPack)
        { }

        // needed for serialization
        protected TreeEntity()
        { }

        public override void Initialize()
        {
            base.Initialize();
            //Heal legacy trees left by pre-fix Scorpion harvests: all pieces collected at invalid positions caused SendInitialState to emit an empty trunks array and the client to render a phantom standing tree. Snap to the root position so the next SendInitialState carries valid pieces and the client's DestroyTrunk path runs.
            if (this.Fallen)
                foreach (var piece in this.trunkPieces)
                    if (piece.Collected && !piece.IsValid) { piece.Position = this.Position; piece.Rotation = Quaternion.Identity; }
            this.UpdateGrowthOccupancy();

            if (this.Fallen) return; //Fallen trees do not get a minimap icon.

            //Construct via the bulk ctor so the initial Position/Type/Scale/DisplayName/DisplayObjectCategory writes do not fire PropertyChanged during world load.
            this.minimapObject = new MinimapObject(this.Species.GetType(), this.Position, this.ComputeMinimapScale(), this.Species.DisplayName, Localizer.DoStr("Trees"));
            MinimapManager.Obj.QueueForBulkRegister(this.minimapObject);
        }

        //The scale formula for the minimap is based on the Species's Scale and the Tree's Growth.
        void UpdateMinimapObjectScale()
        {
            if (this.Species == null || this.Fallen || this.minimapObject == null) return;
            var newScale = this.ComputeMinimapScale();
            if (this.minimapObject.Scale != newScale) this.minimapObject.Scale = newScale;
        }

        Vector3 ComputeMinimapScale()
        {
            if (!this.baseScaleComputed) //Species range and random are constant per tree; compute once.
            {
                var scaleXZ            = this.Species.XZScaleRange.Interpolate(this.scaleRandomValue);
                var scaleY             = this.Species.YScaleRange.Interpolate(this.scaleRandomValue);
                this.baseScale         = new Vector3(scaleXZ, scaleY, scaleXZ);
                this.baseScaleComputed = true;
            }
            //Growth quantized to 5% steps: continuous growth changed every tree's scale each growth tick, flooding clients with per-tree minimap notifications. Clamped min so newborn trees stay visible.
            var quantizedGrowth = MathF.Round(this.GrowthPercent * 20f) / 20f;
            return this.baseScale * Mathf.Lerp(0.25f, 1f, quantizedGrowth);
        }

        void CheckDestroy()
        {
            // destroy the tree if it has fallen, all the trunk pieces are collected, and the stump is removed
            if (this.Fallen && this.stumpHealth <= 0 && this.trunkPieces.All(piece => piece.IsCollectedOrNotValid))
                this.Destroy();
        }

        /// <summary> Auto-slice the trunk into pickup-sized segments. Used by Logger's Luck talent.
        /// Attempts slices at regular intervals across the full trunk span. <see cref="TrySliceTrunkStrict"/>
        /// handles min-size clamping, piece lookup, and client RPC for each slice point. </summary>
        void AutoSliceTrunk()
        {
            lock (this.sync)
            {
                // Target piece length in [0,1] trunk-space that yields MaxTrunkPickupSize resources
                var targetLength = (float)MaxTrunkPickupSize / this.ResourceMultiplier;
                if (targetLength >= 1f) return; // Trunk already fits in one pickup — no slicing needed

                // Slice at regular intervals. TrySliceTrunkStrict finds the correct piece for each
                // point and handles all edge cases (min-size clamping, too-small rejection).
                for (var slicePoint = targetLength; slicePoint < 1f; slicePoint += targetLength)
                    this.TrySliceTrunkStrict(null, slicePoint);
            }
        }

        void PickupLog(Player player, Guid logID, Vector3 pickupPosition)
        {
            lock (this.sync)
            {
                if (!this.CanHarvest)
                    player.ErrorLocStr("Log is not ready for harvest.  Remove all branches first.");

                var trunk = this.trunkPieces.FirstOrDefault(p => p.ID == logID);
                if (trunk?.IsCollectedOrNotValid == false)
                {
                    var resourceType = this.Species.ResourceItemType;
                    var resource     = Item.Get(resourceType);
                    var baseCount    = this.GetBasePickupSize(trunk);
                    var yield        = resource.Yield;
                    var bonusItems   = yield?.GetCurrentValueInt(player.User.DynamicValueContext, null) ?? 0;
                    var numItems     = baseCount + bonusItems;
                    var carried      = player.User.Inventory.Carried;

                    if (numItems > 0)
                    {
                        if (!carried.IsEmpty) // Early tests: neeed to check type mismatch and max quantity.
                        {
                            if      (carried.Stacks.First().Item.Type != resourceType)                    { player.Error(Localizer.Format("You are already carrying {0:items} and cannot pick up {1:items}.", carried.Stacks.First().Item.UILink(LinkConfig.ShowPlural), resource.UILink(LinkConfig.ShowPlural)));  return; }
                            else if (carried.Stacks.First().Quantity + numItems > resource.MaxStackSize)  { player.Error(Localizer.Format("You can't carry {0:n0} more {1:items} ({2} max).", numItems, resource.UILink(numItems != 1 ? LinkConfig.ShowPlural : 0), resource.MaxStackSize));                        return; }
                        }

                        // Prepare a game action pack.
                        var pack = new GameActionPack();
                            pack.AddPostEffect          (() => { trunk.Collected = true; this.RPC("DestroyLog", logID); this.MarkDirty(); this.CheckDestroy(); }); // Delete the log if succseeded.
                            pack.GetOrCreateInventoryChangeSet   (carried, player.User).AddItemsNonUnique(this.Species.ResourceItemType, numItems);                         // Add items to the changeset.
                            pack.AddGameAction          (new HarvestOrHunt() {   Species         = this.Species.GetType(),
                                                                                 HarvestedStacks = new ItemStack(Item.Get(this.Species.ResourceItemType), numItems).SingleItemAsEnumerable(),
                                                                                 ActionLocation  = pickupPosition.XYZi(),
                                                                                 Citizen         = player.User,
                                                                                 ChopperUserID   = this.ChopperUserID});
                            pack.TryPerform(player.User); // Try to perform the action and apply changes & effects.
                    }
                }
            }
        }


        #region RPCs
        //Was [RPC] public void DestroyLeaf(int leafID) with no caller / range / auth check — any client could destroy any
        //tree's leaves anywhere. The first Player parameter is auto-injected by the dispatcher (client wire format unchanged).
        //Caller must either be the assigned tree Controller (matches CollideWithTerrain gate above) or the active chopper.
        [RPC]
        public void DestroyLeaf(Player player, int leafID)
        {
            if (!this.PlayerCanChop(player)) return;
            this.DestroyLeafInternal(leafID);
        }

        void DestroyLeafInternal(int leafID)
        {
            this.GetLeafByID(leafID, out var branchID, out var leafIdOnBranch);
            TreeBranch branch = this.branches[branchID];
            LeafBunch leaf    = branch.Leaves[leafIdOnBranch];

            if (leaf.Health > 0)
            {
                // replicate to all clients
                leaf.Health = 0;
                this.RPC("DestroyLeaves", branchID, leafIdOnBranch);
            }
        }

        [RPC]
        public void DestroyBranch(Player player, int branchID)
        {
            if (!this.PlayerCanChop(player)) return;
            this.DestroyBranchInternal(branchID);
        }

        void DestroyBranchInternal(int branchID)
        {
            TreeBranch branch = this.branches[branchID];

            if (branch.Health > 0)
            {
                // replicate to all clients
                branch.Health = 0;
                this.RPC("DestroyBranch", branchID);
            }

            this.MarkDirty();
        }

        //Gate predicate for the leaf/branch destruction RPCs above. Lets the call through if the caller is the assigned
        //tree controller (already used by CollideWithTerrain), the active chopper (ChopperUserID), or — to handle the
        //first-axe-swing race before controller is assigned — a player within interact range carrying an axe.
        bool PlayerCanChop(Player player)
        {
            if (player == null) return false;
            if (player == this.Controller) return true;
            if (this.ChopperUserID == player.User?.Id) return true;
            if (player.TooFarWithFudgeAndNotify(this.Position.XYZi(), Shared.Items.DefaultInteractDistance.Default)) return false;
            return player.User?.Inventory?.Toolbar?.SelectedStack?.Item is AxeItem;
        }

        TrunkPiece GetTrunkPiece(float slicePoint) => this.trunkPieces.FirstOrDefault(p => p.SliceStart < slicePoint && p.SliceEnd > slicePoint);

        [RPC]
        public void TrySliceTrunkStrict(Player player, float slicePoint, bool ensurePickable = false)
        {
            lock (this.sync) // prevent threading issues due to multiple choppers
            {
                // find the trunk piece this is coming from
                var trunkPiece = this.GetTrunkPiece(slicePoint);
                if (trunkPiece == null) return;

                // if this is a tiny slice, clamp to the nearest valid size
                const float minPieceResources = 5f;
                var         minPieceSize      = minPieceResources / this.ResourceMultiplier;

                //Snap slice to nearest cut that produces max-length pickupable logs
                if (ensurePickable && this.ResourceMultiplier > minPieceResources)
                    slicePoint = minPieceSize;

                var         targetSize        = trunkPiece.SliceEnd - trunkPiece.SliceStart;
                var         targetResources   = targetSize * this.ResourceMultiplier;
                var         newPieceSize      = trunkPiece.SliceEnd - slicePoint;
                var         newPieceResources = newPieceSize * this.ResourceMultiplier;
                if (targetResources <= minPieceResources) return;          // can't slice, too small

                if (targetResources < (2 * minPieceResources))               slicePoint = trunkPiece.SliceStart + (.5f * targetSize); // if smaller than 2x the min size, slice directly in half
                else if (newPieceSize < minPieceSize)                        slicePoint = trunkPiece.SliceEnd - minPieceSize;         // round down to nearest slice point where the resulting block will be the size of the log
                else if (slicePoint - trunkPiece.SliceStart <= minPieceSize) slicePoint = trunkPiece.SliceStart + minPieceSize;       // round up

                var sourceID = trunkPiece.ID;
                // slice and assign new IDs (New piece is always the back end of the source piece)
                var newPiece = new TrunkPiece()
                {
                    ID         = Guid.NewGuid(),
                    SliceStart = slicePoint,
                    SliceEnd   = trunkPiece.SliceEnd,
                    Position   = trunkPiece.Position,
                    Rotation   = trunkPiece.Rotation,
                };
                this.trunkPieces.Add(newPiece);
                trunkPiece.ID       = Guid.NewGuid();
                trunkPiece.SliceEnd = slicePoint;

                // ensure the pieces are listed in order
                this.trunkPieces.Sort((a, b) => a.SliceStart.CompareTo(b.SliceStart));

                // reciprocate to clients
                this.RPC("SliceTrunk", slicePoint, sourceID, trunkPiece.ID, newPiece.ID);

                UseVehicleToolDurability(player, SliceDurabilityCost);

                PlantSimEvents.OnLogChopped.Invoke((player as Player)?.User);

                this.MarkDirty();
                return;
            }
        }

        [RPC]
        public void CheckVehicleHarvestDebris(Player player, Vector3i position)
        {
            //Scatter debris around the harvested tree by reusing the chopping debris spawn (CollideWithTerrain).
            const int radius = 2;
            for (var i = 0; i < 8; i++)
            {
                //Pick a random column within the radius and snap to the empty block above the ground there,
                //so CollideWithTerrain always gets a non-solid spot sitting on top of solid terrain.
                var randomColumn = position.XZ + new Vector2i(RandomUtil.Range(-radius, radius + 1), RandomUtil.Range(-radius, radius + 1));
                this.CollideWithTerrain(player, World.GetTopEmptyPos(randomColumn));
            }
        }

        [RPC]
        public void CollideWithTerrain(Player player, Vector3i position)
        {
            if (player != this.Controller)
                return;

            lock (this.sync)
            {
                if (this.groundHits == null)
                    this.groundHits = new ThreadSafeHashSet<Vector3i>();
            }

            // Prevent spawning more than MaxGroundHits debris for one tree
            if (this.treeDebrisSpawned >= MaxTreeDebris)
                return;

            // destroy plants and spawn dirt within a radius under the hit position
            var radius = 1;
            for (var x = -radius; x <= radius; x++)
                for (var z = -radius; z <= radius; z++)
                {
                    var offsetpos = position + new Vector3i(x, -1, z);
                    if (!this.groundHits.Add(offsetpos))
                        continue;

                    var abovepos = offsetpos + Vector3i.Up;
                    var aboveblock = World.GetBlock(abovepos);
                    var hitblock = World.GetBlock(offsetpos);
                    if (!aboveblock.Is<Solid>())
                    {
                        // turn soil into dirt
                        if (hitblock.GetType() == typeof(GrassBlock) || hitblock.GetType() == typeof(ForestSoilBlock))
                        {
                            player.SpawnBlockEffect(offsetpos, typeof(DirtBlock), BlockEffect.Delete);
                            World.SetBlock<DirtBlock>(offsetpos);
                            BiomePusher.AddFrozenColumn(offsetpos.XZ);
                        }

                        // kill any above plants
                        if (aboveblock is PlantBlock)
                        {
                            // make sure there is a plant here, sometimes world/ecosim are out of sync
                            var plant = EcoSim.PlantSim.GetPlant(abovepos);
                            if (plant != null)
                            {
                                player.SpawnBlockEffect(abovepos, aboveblock.GetType(), BlockEffect.Delete);
                                EcoSim.PlantSim.DestroyPlant(plant, DeathType.Logging, true, player.User);
                            }
                            else World.DeleteBlock(abovepos);
                        }

                        if (hitblock.Is<Solid>() && World.GetBlock(abovepos).Is<Empty>() && RandomUtil.Value < this.Species.ChanceToSpawnDebris)
                        {
                            GameActionAccumulator.Obj.AddGameActions(new CreateTreeDebris()
                            {
                                Count = 1,
                                ActionLocation = abovepos,
                                Citizen = player.User
                            }, player?.User);
                            var changes = InventoryChangeSet.New(player.User.Inventory, player.User);
                            var debrisResources = this.Species.DebrisResources;
                            debrisResources.ForEach(x => changes.AddItemsNonUnique(x.Key, x.Value.RandIntInc));
                            changes.TryApply();

                            RoomData.QueueRoomTest(abovepos);
                            if (Interlocked.Increment(ref this.treeDebrisSpawned) >= MaxTreeDebris) return;
                        }
                    }
                }
        }
        #endregion

        ChopTree CreateChopTreeAction(INetObject damager, Item tool, bool felled, bool branches = false) => new ChopTree()
        {
            Citizen          = (damager as Player)?.User,
            Species          = this.Species.GetType(),
            ChopperUserID    = this.ChopperUserID,
            ActionLocation   = this.Position.XYZi(),
            AccessNeeded     = AccessType.FullAccess,
            Felled           = felled,
            BranchesTargeted = branches,
            GrowthPercent    = this.GrowthPercent * 100,
            ToolUsed         = tool
        };

        /// <summary> Fells a tree instantly using a vehicle (Scorpion). Called by the client when the Scorpion grabs a standing tree.
        /// Deals enough damage to fell it in one hit, going through the law system via <see cref="GameActionPack"/>.
        /// After felling, the client drives the processing sequence: <see cref="TryStartProcessingTree"/> to begin cutting,
        /// then <see cref="TryVehiclePickup"/> for each trunk piece to store logs in the vehicle's inventory. </summary>
        [RPC]
        public void HarvestTreeWithVehicle(Player player)
        {
            var vehicle = player.MountManager.Mount?.Parent;
            if (vehicle == null) return;
            if (vehicle.GetComponent<ModularVehicleComponent>()?.ToolBrokenNotify(player.User) == true) return;   //A broken tree cutter can't fell

            var toolComponent   = vehicle.GetComponent<VehicleToolComponent>();
            var cutterComponent = vehicle.GetComponent<VehicleTreeHarvestComponent>();

            // The scorpion does not have a vehicle tool component so we separate these up to make sure we either use the specific tool or the parent item
            // in the case of missing the component, this is the same method we use for the stump destruction as well.
            var vehicleTool = toolComponent?.ToolItem as Item ?? vehicle.CreatingItem as Item;

            //Try to interact with tree (fall it through law system)
            var pack = this.TryDamageTrunk(new GameActionPack(), player, this.health * 2, vehicleTool);
            var result = pack.TryPerform(player.User);
            if (result.Failed)
            {
                if (cutterComponent != null) cutterComponent.TreeCutFail(result.Message);
                return;
            }

            toolComponent?.ToolItem?.UseDurability(FellDurabilityCost, player);
        }

        //Saw work wears the vehicle's cutter module, mirroring how the scoop is charged per block dug. Charged per tree plus per cut so a big trunk costs more than a sapling.
        const float FellDurabilityCost  = 1f;
        const float SliceDurabilityCost = 1f;

        /// <summary>Charges the mounted vehicle's tool module for saw work. No-op when there's no driver (talent-driven auto-slicing), when
        /// slicing by hand, or on vehicles with no tool module of their own to wear, such as the Scorpion.</summary>
        static void UseVehicleToolDurability(Player player, float amount)
        {
            if (player == null) return;
            player.MountManager.Mount?.Parent?.GetComponent<VehicleToolComponent>()?.ToolItem?.UseDurability(amount, player);
        }

        /// <summary> Picks up a trunk piece and stores it in the vehicle's inventory. Called by the client for two distinct flows:
        /// <list type="bullet">
        ///   <item><b>Scorpion (VehicleTreeHarvestComponent only, no VehicleToolComponent):</b> The Scorpion grabs the tree, processes it
        ///     internally, and calls this RPC automatically for each trunk piece as it is cut. Trunk pieces are moved by the Scorpion during
        ///     processing, so their world position becomes invalid — we only check <see cref="TrunkPiece.Collected"/>, not
        ///     <see cref="TrunkPiece.IsValid"/>. No size limit: the Scorpion can pick up any trunk size. Uses direct
        ///     <see cref="Inventory.TryAddItemsNonUnique"/> to its <see cref="PublicStorageComponent"/>.</item>
        ///   <item><b>Scoop on SkidSteer/Excavator/SteamTractor (VehicleToolComponent):</b> The Steam Tractor tree cutter fells and cuts
        ///     the tree but drops all trunk pieces on the ground. A vehicle with a scoop attachment then picks them up one by one via this
        ///     RPC. The Steam Tractor has both VehicleTreeHarvestComponent and VehicleToolComponent — the presence of VehicleToolComponent
        ///     routes it through this scoop path. Enforces <see cref="MaxTrunkPickupSize"/> (pieces must be sliced small enough first) and
        ///     goes through <see cref="GameActionPack"/> for law/auth validation. Stores into <see cref="VehicleToolComponent.ToolInventory"/>.</item>
        /// </list>
        /// Note: The Steam Tractor tree cutter itself does NOT call this RPC — it only fells/slices the tree via
        /// <see cref="TryStartProcessingTree"/> and <see cref="TrySliceTrunkStrict"/>. Pickup is left to the scoop. </summary>
        [RPC]
        public void TryVehiclePickup(Player player, string trunkIDString)
        {
            var trunkID = Guid.Parse(trunkIDString);
            var vehicle = player.MountManager.Mount?.Parent;
            if (vehicle == null) { this.NotifyPickupFailed(player, trunkID); return; }

            lock (this.sync)
            {
                var trunk = this.trunkPieces.FirstOrDefault(x => x.ID == trunkID);
                if (trunk == null || trunk.Collected) return;   //Gone or already collected: the client-side log is being destroyed anyway, nothing to restore.

                var (resource, numItems) = this.CalculateTrunkResources(player, trunk);
                if (numItems <= 0) { this.NotifyPickupFailed(player, trunkID); return; }

                // Scorpion path: direct inventory add, no size limit, no GameActionPack.
                // The Scorpion moves trunk pieces during processing so their Position is no longer valid in the world —
                // that's why we check only Collected (not IsCollectedOrNotValid which also checks IsValid/position).
                // Detected by having VehicleTreeHarvestComponent but NOT VehicleToolComponent — the Steam Tractor has both
                // (tree cutter + scoop) and must go through the scoop path below.
                var isHarvesterVehicle = vehicle.GetComponent<VehicleTreeHarvestComponent>() != null
                                      && vehicle.GetComponent<VehicleToolComponent>() == null;
                if (isHarvesterVehicle)
                {
                    var storage = vehicle.GetComponent<PublicStorageComponent>();
                    if (storage == null) { this.NotifyPickupFailed(player, trunkID); return; }

                    var result = storage.Inventory.TryAddItemsNonUnique(this.Species.ResourceItemType, numItems);
                    if (!result) { this.NotifyPickupFailed(player, trunkID); return; } // inventory full, keep the trunk

                    trunk.Collected = true;
                    //Snap position back to the tree's root so the saved piece stays valid. The Scorpion animation left the trunk in claw-local space (invalid world pos); without this, SendInitialState filters it out on next load and the client never runs DestroyTrunk, leaving the tree visually standing though the server knows it's felled.
                    trunk.Position = this.Position;
                    trunk.Rotation = Quaternion.Identity;
                    this.RPC("DestroyLog", trunk.ID);
                    this.MarkDirty();
                    this.CheckDestroy();
                    return;
                }

                // Scoop path (SkidSteer/Excavator): enforce max pickup size and go through GameActionPack for law/auth
                if (this.GetBasePickupSize(trunk) > MaxTrunkPickupSize)
                {
                    player.Error(LogTooLargeMessage);
                    this.NotifyPickupFailed(player, trunkID);
                    return;
                }

                var targetInventory = vehicle.GetComponent<VehicleToolComponent>()?.ToolInventory
                                   ?? vehicle.GetComponent<PublicStorageComponent>()?.Inventory;
                if (targetInventory == null) { this.NotifyPickupFailed(player, trunkID); return; }

                using var pack = new GameActionPack();
                pack.AddPostEffect(() => { trunk.Collected = true; this.RPC("DestroyLog", trunk.ID); this.MarkDirty(); this.CheckDestroy(); });
                pack.GetOrCreateInventoryChangeSet(targetInventory, player.User).AddItemsNonUnique(this.Species.ResourceItemType, numItems);
                pack.AddGameAction(new HarvestOrHunt()
                {
                    Species         = this.Species.GetType(),
                    HarvestedStacks = new ItemStack(Item.Get(this.Species.ResourceItemType), numItems).SingleItemAsEnumerable(),
                    ActionLocation  = trunk.Position.XYZi(),
                    Citizen         = player.User,
                    ChopperUserID   = this.ChopperUserID,
                });

                if (pack.TryPerform(player.User).Failed) this.NotifyPickupFailed(player, trunkID);   //Fallback for the inventory action: full inventory, law rejection, etc.
            }
        }

        /// <summary>The pickup didn't complete — tell the requesting client so it can restore the trunk it optimistically hid.</summary>
        void NotifyPickupFailed(Player player, Guid trunkID) => this.RPC("VehiclePickupFailed", player.Client, trunkID);

        (Item Item, int Amount) CalculateTrunkResources(Player player, TrunkPiece trunk)
        {
            var resourceType = this.Species.ResourceItemType;
            var resource     = Item.Get(resourceType);
            var baseCount    = this.GetBasePickupSize(trunk);
            var yield        = resource.Yield;
            var bonusItems   = yield?.GetCurrentValueInt(player.User.DynamicValueContext, null) ?? 0;
            var numItems     = baseCount + bonusItems;

            // Apply talent harvest yield bonuses (e.g. Natural Gatherer +1)
            numItems = (int)BonusManager.ApplyBonuses(player.User.MakeBonusContext(BonusAction.HarvestYield, resource), numItems);

            return (resource, numItems);
        }

        [RPC]
        public void TryStartProcessingTree(Player player, bool checkStorage)
        {
            var vehicle = player.MountManager.Mount?.Parent;
            if (vehicle == null) return;
            if (vehicle.GetComponent<ModularVehicleComponent>()?.ToolBrokenNotify(player.User) == true) return;   //A broken tree cutter can't slice/process

            var harvester = vehicle.GetComponent<VehicleTreeHarvestComponent>();
            if (harvester == null) return;

            if (checkStorage)
            {
                var storage = vehicle.GetComponent<PublicStorageComponent>();
                var (item, amount) = this.CalculateTrunkResources(player, new TrunkPiece { SliceStart = 0f, SliceEnd = 1f });

                //Check if inventory can accept all tree resources
                using (var changes = InventoryChangeSet.New(storage.Inventory, player.User))
                {
                    changes.AddItemsNonUnique(item.Type, amount);
                    if (changes.CanApplyNonDisposing().Success)
                    {
                        harvester.ProcessTree(); // success, start cut
                    }
                    else player.User.ErrorLocStr("Not enough space in destination inventory.");
                }
            }
            else harvester.ProcessTree();
        }

        [RPC]
        public void FreezeTrunk(Player player, Guid trunkID, bool freeze)
        {
            var trunk = this.trunkPieces.FirstOrDefault(x => x.ID == trunkID);
            if (trunk != null) this.RPC("FreezeTrunk", trunkID, freeze);
        }
        
        public override void FellTree(INetObject killer)
        {
            // Ensure values are set correctly
            this.health = 0;
            
            // create the initial trunk piece
            var trunkPiece = new TrunkPiece() { ID = Guid.NewGuid(), SliceStart = 0f, SliceEnd = 1f,  };

            // clear tree occupancy
            if (this.Species.BlockType != null)
            {
                var treeBlockCheck = this.Position.XYZi() + Vector3i.Up;
                while (World.GetBlock(treeBlockCheck).GetType() == this.Species.BlockType)
                {
                    World.DeleteBlock(treeBlockCheck);
                    treeBlockCheck += Vector3i.Up;
                }
            }

            this.trunkPieces.Add(trunkPiece);

            var player = killer as Player;
            if (player != null)
            {
                this.SetPhysicsController(player);                        // set the killing player's client as the one in control of the physics of the tree. Handled by "FellTree".
                player.RPC("YellTimber");                                 // Issue sound effect.
            }

            this.RPC("FellTree", trunkPiece.ID, this.ResourceMultiplier, killer is Player); // Fell the tree
            Animal.AlertNearbyAnimals(this.Position, 15f);                // Alert nearby animals to aware about falling tree

            var isVehicleCut = player?.MountManager.Mount?.Parent?.GetComponent<VehicleTreeHarvestComponent>() != null;

            // break off any branches that are young
            if (killer is Player && !isVehicleCut)
            {
                for (var branchID = 0; branchID < this.branches.Length; branchID++)
                {
                    var branch = this.branches[branchID];
                    if (branch == null)
                        continue;

                    var branchAge = Mathf.Clamp01((float)((this.GrowthPercent - branch.SpawnAge) / (branch.MatureAge - branch.SpawnAge)));
                    if (branchAge <= .5f)
                        this.DestroyBranchInternal(branchID);
                }
            }

            if (player != null)
                PlantSimEvents.TreeFelledEvent.Invoke(player.User, this.Species);
            MinimapManager.Obj.UnregisterMinimapObject(this.minimapObject); //Cancels a pending bulk-add if not yet drained, else removes from the delta set. Null-safe.

            // Logger's Luck: single chance proc — auto-slice trunk into pickup-sized pieces + destroy stump (boolean-query pattern).
            // Deferred so the client has time to process FellTree RPC and let the tree land before receiving slice/destroy RPCs.
            // Relic species (old growth) are exempt: processing a landmark tree should stay a manual effort.
            if (player != null && !isVehicleCut && !this.Species.NoSpread)
            {
                var ctx = player.User.MakeBonusContext(BonusAction.AutoProcessLog);
                if (BonusManager.ApplyBonuses(ctx, 0f) > 0f)
                {
                    //Capture the tool held at proc time so tool-filtered laws match the ChopStump action. Captured outside the delay to pin it to the swing that triggered the proc, not whatever is in hand 5s later.
                    var heldTool = player.User.Inventory.Toolbar.SelectedItem as ToolItem;
                    ThreadUtils.Delay(5f, () =>
                    {
                        this.AutoSliceTrunk();
                        this.TryDestroyStump(player, heldTool);
                    });
                }
            }

            this.MarkDirty();
        }

        public GameActionPack TryApplyDamage(GameActionPack pack, INetObject damager, float amount, InteractionTarget target, Item tool, out float damageReceived, Type damageDealer = null, float experienceMultiplier = 1f)
        {
            damageReceived = amount;

            //If the tree is really young, just outright uproot and destroy it
            if (this.IsSapling) return this.TryKillSapling(pack, damager, tool);
            else if (target.TryGetParameter(BlockParameterNames.IsTrunk, out _))              return this.TryDamageTrunk (pack, damager, amount, tool);
            else if (target.TryGetParameter(BlockParameterNames.IsStump, out _))              return this.TryDamageStump (pack, damager, amount, tool);
            else if (target.TryGetParameter(BlockParameterNames.Branch,  out var branchID))   return this.TryDamageBranch(pack, damager, amount, (int)branchID,     tool);
            else if (target.TryGetParameter(BlockParameterNames.Slice,   out var slicePoint)) return this.TrySliceTrunk  (pack, damager, amount, (float)slicePoint, tool);
            else if (target.TryGetParameter(BlockParameterNames.Leaf,    out var leafID))     return this.TryDamageLeaf  (pack, damager, amount, (int)leafID,       tool);
    
            DebugUtils.Fail($"Failed to damage a tree - tree is not a sapling and we can't determine which part of the tree should be damaged because of missing parameters in {nameof(target)}.");
            pack.EarlyResult = Result.FailedNoMessage;
            return pack;
        }

        /// <summary>Try to destroy stump by applying full damage.</summary>
        public bool TryDestroyStump(Player damager, Item tool)
        {
            var action = this.TryDamageStump(new(), damager, this.stumpHealth, tool, false);
            return action.TryPerform(damager.User).Success;
        }

        /// <summary>Perform damaging healthy branches and trunk (if it's a fallen tree).</summary>
        private GameActionPack TrySliceTrunk(GameActionPack pack, INetObject damager, float amount, float slicePoint, Item tool)
        {
            //If there are still branches, damage them instead
            for (var branchID = 0; branchID < this.branches.Length; branchID++)
                this.TryDamageBranch(pack, damager, amount, branchID, tool);

            //If the tree is fallen, damage it
            if (this.Fallen) pack.AddGameAction(this.CreateChopTreeAction(damager, tool, false));
        
            //Cut into slices if action is a success
            pack.AddPostEffect(() => this.TrySliceTrunkStrict(damager as Player, slicePoint));
            return pack;
        }

        private GameActionPack TryKillSapling(GameActionPack pack, INetObject damager, Item tool)
        {
            pack.AddGameAction(this.CreateChopTreeAction(damager, tool, true));
            pack.AddPostEffect(() => EcoSim.PlantSim.DestroyPlant(this, DeathType.Harvesting, killer:damager is Player damagerPlayer ? damagerPlayer.User : null));
            return pack;
        }

        private GameActionPack TryDamageTrunk(GameActionPack pack, INetObject damager, float amount, Item tool)
        {
            if (this.health <= 0) return pack;

            pack.AddGameAction(this.CreateChopTreeAction(damager, tool, this.health <= amount));

            pack.AddPostEffect(() =>
            { 
                // damage trunk
                var damageDone = InterlockedUtils.SubMinNonNegative(ref this.health, amount);
                if (damageDone <= 0f) return;

                this.RPC("UpdateHP", this.health / this.Species.TreeHealth);

                if (this.health <= 0)
                {
                    this.FellTree(damager);
                    this.ChopperUserID = damager is Player player ? player.User.Id : -1;
                    EcoSim.PlantSim.KillPlant(this, DeathType.Logging, true); //Records the loss on the species population layer, whoever felled it
                
					
                    if (World.GetBlock(this.Position.XYZi()).GetType() == this.Species.BlockType) World.DeleteBlock(this.Position.XYZi());
                    this.stumpHealth = 0;
                
                    this.RPC("DestroyStump");

                    // Let another plant grow here
                    EcoSim.PlantSim.UpRootPlant(this);
           }

                this.MarkDirty();
            });
            return pack;
        }

        private GameActionPack TryDamageStump(GameActionPack pack, INetObject damager, float amount, Item tool, bool giveResource = true)
        {
            if (this.Fallen && this.stumpHealth > 0)
            {
                var player = damager as Player;
                if (player != null)
                {
                    pack.AddGameAction(new ChopStump()
                    {
                        Citizen        = player.User,
                        ActionLocation = this.Position.XYZi(),
                        Destroyed      = this.stumpHealth <= amount,
                        Species        = this.Species.GetType(),
                        ToolUsed       = tool
                    });
                }

                pack.AddPostEffect(() =>
                {
                    this.stumpHealth = Mathf.Max(0, this.stumpHealth - amount);

                    if (this.stumpHealth <= 0)
                    {
                        if (World.GetBlock(this.Position.XYZi()).GetType() == this.Species.BlockType) World.DeleteBlock(this.Position.XYZi());
                        this.stumpHealth = 0;
                        //give tree resources
                        if (player != null && giveResource)
                        {
                            var changes = InventoryChangeSet.New(player.User.Inventory, player.User);
                            var trunkResources = this.Species.TrunkResources;
                            if (trunkResources != null) trunkResources.ForEach(x => changes.AddItemsNonUnique(x.Key, x.Value.RandIntInc));
                            else DebugUtils.Fail("Trunk resources missing for: " + this.Species.Name);
                            changes.TryApply();
                        }
                        this.RPC("DestroyStump");

                        // Let another plant grow here
                        EcoSim.PlantSim.UpRootPlant(this);
                    }

                    this.MarkDirty();
                    this.CheckDestroy();
                });
            }
            else this.TryDamageTrunk(pack, damager, amount, tool);

            return pack;
        }

        private GameActionPack TryDamageLeaf(GameActionPack pack, INetObject damager, float amount, int leafID, Item tool)
        {
            var leaf = this.GetLeafByID(leafID, out var branchID, out var leafIdOnBranch);
            if (leaf != null && leaf.Health > 0)
            {
                pack.AddGameAction(this.CreateChopTreeAction(damager, tool, false, true));
                pack.AddPostEffect(() =>
                {
                    if ((leaf.Health = Mathf.Max(0, leaf.Health - amount)) == 0) this.RPC("DestroyLeaves", branchID, leafIdOnBranch);
                    this.MarkDirty();
                });
            }
        
            return pack;
        }

        /// <summary>LeafID on the client is unique for the whole tree not just the branch, this takes that unique id and returns the leaf while outputting ID of the branch the leaf is on and leaf ID relative to the branch</summary>
        LeafBunch GetLeafByID(int leafID, out int branchID, out int leafIdOnBranch)
        {
            branchID = 0;
            for (; branchID < this.branches.Length; branchID++) //Iterate through all of the branches
            {
                var branch = this.branches[branchID];                         //Get the branch so we can check its leaves
                if (branch == null) continue;                                 //if the branch is null it has no leaves (trees can have null branches for variety)
                var branchLeavesCount = branch.Leaves.Length;                 //since leaf ID is based on amount of total leaves on the tree, if LeafID is greater than total leaf count on that branch its not the correct branch
                if (leafID >= branchLeavesCount) leafID -= branchLeavesCount; //so we reduce leafID by the leaf count of that branch (to do same check for the next branch) and increment branch ID
                else break;                                                   //do this until we are left with a branch where leaf count is more than the modified leaf ID, that is the parent branch of our leaf
            }
            var leaf = this.branches[branchID].Leaves[leafID];
            leafIdOnBranch = leafID;
            return leaf;
        }

        private GameActionPack TryDamageBranch(GameActionPack pack, INetObject damager, float amount, int branchID, Item tool)
        {
            var branch = this.branches[branchID];
            if (branch != null && branch.Health > 0)
            {
                pack.AddGameAction(this.CreateChopTreeAction(damager, tool, false, true));
                pack.AddPostEffect(() =>
                { 
                    if ((branch.Health = Mathf.Max(0, branch.Health - amount)) == 0) { this.RPC("DestroyBranch", branchID); this.MarkDirty(); }
                });
            }
            return pack;
        }

        public override void SendInitialState(BSONObject bsonObj, INetObjectViewer viewer)
        {
            base.SendInitialState(bsonObj, viewer);

            // if we have trunk pieces, send those
            this.trunkPieces.RemoveNulls();
            var trunkInfo = BSONArray.New;
            if (this.trunkPieces.Count > 0)
            {
                foreach (var trunkPiece in this.trunkPieces)
                    if (trunkPiece.IsValid) trunkInfo.Add(trunkPiece.ToInitialBson());
            }
            bsonObj["trunks"] = trunkInfo;

            if (this.Controller != null && this.Controller is INetObject)
                bsonObj["controller"] = ((INetObject)this.Controller).ID;

            bsonObj["mult"] = this.ResourceMultiplier;
            bsonObj["density"] = this.Species.Density;
        }

        public override void SendUpdate(BSONObject bsonObj, INetObjectViewer viewer)
        {
            base.SendUpdate(bsonObj, viewer);

            if (this.Fallen && this.Controller != viewer)
            {
                var trunkInfo = BSONArray.New;
                foreach (var trunkPiece in this.trunkPieces)
                {
                    if (trunkPiece.Position == Vector3.Zero || !trunkPiece.IsValid)
                        continue;
                    if (trunkPiece.LastUpdateTime < viewer.LastSentUpdateTime)
                        continue;

                    trunkInfo.Add(trunkPiece.ToUpdateBson());
                }
                bsonObj["trunks"] = trunkInfo;
                bsonObj["time"] = this.lastKeyframeTime;
            }
        }

        public override void ReceiveUpdate(BSONObject bsonObj)
        {
            bool changed = false;
            if (!bsonObj.ContainsKey("trunks"))
                return;
            BSONArray trunks = bsonObj["trunks"].ArrayValue;
            foreach (BSONObject obj in trunks)
            {
                Guid id = obj["id"];
                TrunkPiece piece = this.trunkPieces.FirstOrDefault(p => p.ID == id);

                if (piece != null && (piece.Position != obj["pos"] || piece.Rotation != obj["rot"]))
                {
                    piece.Position = obj["pos"];
                    piece.Rotation = obj["rot"];
                    if (obj.ContainsKey("v")) piece.Velocity = obj["v"];
                    piece.LastUpdateTime = TimeUtil.Seconds;
                    changed = true;
                }
            }

            if (changed)
            {
                this.lastKeyframeTime = bsonObj["time"];
                this.LastUpdateTime = TimeUtil.Seconds;
            }
        }

        #region mostly copied from NetPhysicsEntity
        public override bool IsRelevant(INetObjectViewer viewer)
        {
            if (viewer is IWorldObserver observer)
            {
                var closestWrapped = World.ClosestWrappedLocation(observer.Position, this.Position);
                var v = closestWrapped - observer.Position;
                if (World.WrappedDistanceSq(this.Position, observer.Position) < observer.ChunkViewDistance.VisibleSq)
                {
                    if (this.Controller == null)
                        this.SetPhysicsController(observer);
                    return true;
                }
            }
            return false;
        }

        public override bool IsNotRelevant(INetObjectViewer viewer)
        {
            if (viewer is not IWorldObserver observer) return false;                                   // only can check for IWorldObserver
            var closestWrapped     = World.ClosestWrappedLocation(observer.Position, this.Position);
            var notVisibleDistance = observer.ChunkViewDistance.NotVisible;
            var v                  = closestWrapped - observer.Position;
            if (Mathf.Abs(v.X) >= notVisibleDistance || Mathf.Abs(v.Z) >= notVisibleDistance)         // check if any of horizontal distances to viewer length enough to go out of view
            {
                if (this.Controller != null && this.Controller.Equals(viewer))                        // in that case reset owning physics controller
                    this.SetPhysicsController(null);
                return true;                                                                          // and return true
            }
            else
            {
                // Still relevant, Check if viewer would be a better controller (observer is very close and current controller is far enough)
                if (this.Controller is not IWorldObserver current)
                    this.SetPhysicsController(observer);
                else if (current != observer && Vector2.WrappedDistance(observer.Position.XZ(), this.Position.XZ()) < 10f)
                {
                    if (Vector2.WrappedDistance(current.Position.XZ(), this.Position.XZ()) > 15f)
                        this.SetPhysicsController(observer);
                }
                return false;
            }
        }

        public bool SetPhysicsController(INetObjectViewer owner)
        {
            // Trees don't need physics until felled
            if (!this.Fallen)
                return false;

            if (Equals(this.Controller, owner))
                return false;

            this.Controller?.RemoveDestroyAction(this.RemovePhysicsController);

            this.Controller = owner;

            this.Controller?.AddDestroyAction(this.RemovePhysicsController);

            if (owner is INetObject netObject)
                this.NetObj.Controller.RPC("UpdateController", netObject.ID);

            return true;
        }

        void RemovePhysicsController()
        {
            this.SetPhysicsController(null);
        }
        #endregion

        /// <summary> Check if we need to send the update to client based on update time. </summary>
        public override bool IsUpdated(INetObjectViewer viewer)
        {
            if (this.LastUpdateTime > viewer.LastSentUpdateTime) return true;
            return false;
        }

        public override void Destroy()
        {
            var treeBlockCheck = this.Position.XYZi();
            for (int i = 0; i <= GrowthThresholds.Length; i++)
            {
                var block = World.GetBlock(treeBlockCheck);
                if (block.GetType() == this.Species.BlockType)
                    World.DeleteBlock(treeBlockCheck);
                if (block.Is<Solid>())
                    break;
                treeBlockCheck += Vector3i.Up;
            }
            MinimapManager.Obj.UnregisterMinimapObject(this.minimapObject); //Cancels a pending bulk-add if not yet drained, else removes from the delta set. Null-safe.
            base.Destroy();
        }
    }
}
