// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Eco.Mods.TechTree
{
    using System.ComponentModel;
    using Eco.Core.Controller;
    using Eco.Core.Items;
    using Eco.Gameplay.DynamicValues;
    using Eco.Gameplay.GameActions;
    using Eco.Gameplay.Interactions.Interactors;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Players;
    using Eco.Shared.Items;
    using Eco.Shared.Localization;
    using Eco.Shared.Serialization;
    using Eco.Shared.SharedTypes;
    using Eco.Shared.Utils;
    using Eco.World.Blocks;

    [Serialized]
    [LocDisplayName("Hammer")]
    [LocDescription("Used to construct buildings and pickup manmade objects.")]
    [Category("Hidden")]

    // IMPORTANT:
    // Block-form selection uses the tool's TierAttribute.
    // Tier 4 is the Modern Hammer tier, so every hammer derived from
    // HammerItem receives the complete Modern Hammer form wheel.
    [Tier(4)]
    public abstract class HammerItem : BuildingToolItem
    {
        private static readonly IDynamicValue caloriesBurn =
            new ConstantValue(1);

        private static readonly IDynamicValue skilledRepairCost =
            new ConstantValue(1);

        public override IDynamicValue SkilledRepairCost =>
            skilledRepairCost;

        // Return the tier calculated from TierAttribute.
        // Because HammerItem now has [Tier(4)], all derived hammers
        // are treated as Modern Hammer tier.
        [SyncToView]
        public override IDynamicValue Tier =>
            base.Tier;

        public override IDynamicValue CaloriesBurn =>
            caloriesBurn;

        public override bool IsValidForInteraction(Item item)
        {
            var blockItem = item as BlockItem;

            return !(item is LogItem)
                && blockItem != null
                && Block.Is<Constructed>(blockItem.OriginType);
        }

        // Fast deconstruction with the interaction key.
        // Default key: English E.
        [Interaction(
            InteractionTrigger.InteractKey,
            tags: "Constructable",
            canHoldToTrigger: TriBool.True,
            animationDriven: false,
            Priority = 100)]
        public bool RemoveFastWithE(
            Player player,
            InteractionTriggerInfo triggerInfo,
            InteractionTarget target)
        {
            var foundBlocks = this.TryCreateMultiblockContext(
                out var blockContext,
                target,
                player,
                tagsTargetable: "Constructable",
                mustHaveTags: BlockTags.NonPlant.SingleItemAsEnumerable(),
                applyXPSkill: false);

            if (!foundBlocks)
                return false;

            using var pack = new GameActionPack();

            pack.DeleteBlock(
                blockContext,
                player.User.Inventory);

            return pack.TryPerform(player.User).Success;
        }
    }
}
