using System;
using System.Collections.Generic;
using Eco.Core.Items;
using Eco.Gameplay.Components;
using Eco.Gameplay.Components.Auth;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Shared.Math;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Mods.TechTree;
using Eco.Gameplay.Skills;
using System.Linq;
using Eco.Core.Controller;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Shared.Items;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Occupancy;
using Eco.Gameplay.Items.Recipes;
using static Eco.Gameplay.Components.PartsComponent;

namespace Eco.EM.Machines.Trucking
{ 
    [Serialized]
    [LocDisplayName("Semi Truck - Tanker Trailer")]
    [Weight(25000)]
    [AirPollution(0.5f)]
    [Ecopedia("Crafted Objects", "Vehicles", createAsSubPage: true)]
    [LocDescription("Modern Truck With a Semi Trailer attached for transporting Fuel")]
    public partial class SemiTruckTankerItem : WorldObjectItem<SemiTruckTankerObject>, IPersistentData
    {
        [Serialized, SyncToView, NewTooltipChildren(CacheAs.Instance, flags: TTFlags.AllowNonControllerTypeForChildren)] public object PersistentData { get; set; }
    }

    [ForceCreateView]
    public partial class SemiTruckTankerRecipe : Recipe
    {
        public SemiTruckTankerRecipe()
        {
            this.Init(
                name: "Semi Truck - Tanker Trailer",  //noloc
                    displayName: Localizer.DoStr("Semi Truck - Tanker Trailer"),

                    // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                    // type of the item, the amount of the item, the skill required, and the talent used.
                    ingredients: new List<IngredientElement>
                    {
                    new IngredientElement(typeof(TankerTrailerItem), 1, true),
                    new IngredientElement(typeof(PrimeMoverItem), 1, true),
                    },

                    // Define our recipe output items.
                    // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                    // to create.
                    items: new List<CraftingElement>
                    {
                    new CraftingElement<SemiTruckTankerItem>()
                });

            // Perform pre/post initialization for user mods and initialize our recipe instance with the display name "Truck"
            this.ModsPostInitialize();

            // Register our RecipeFamily instance with the crafting system so it can be crafted.
            CraftingComponent.AddTagProduct(typeof(RoboticAssemblyLineObject), typeof(SemiTruckRecipe), this);

        }

        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();

        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();

    }

    [Serialized]
    [RequireComponent(typeof(StandaloneAuthComponent))]
    [RequireComponent(typeof(FuelSupplyComponent))]
    [RequireComponent(typeof(FuelConsumptionComponent))]
    [RequireComponent(typeof(PublicStorageComponent))]
    [RequireComponent(typeof(MovableLinkComponent))]
    [RequireComponent(typeof(AirPollutionComponent))]
    [RequireComponent(typeof(VehicleComponent))]
    [RequireComponent(typeof(ModularStockpileComponent))]
    [RequireComponent(typeof(CustomTextComponent))]
    [RequireComponent(typeof(MinimapComponent))]
    [RequireComponent(typeof(PaintableComponent))]
    [RequireComponent(typeof(PartsComponent))]
    public partial class SemiTruckTankerObject : PhysicsWorldObject, IRepresentsItem
    {
        public override LocString DisplayName => Localizer.DoStr("Tanker Truck");
        public Type RepresentedItemType => typeof(SemiTruckTankerItem);

        public override float InteractDistance => 16f;
        public override TableTextureMode TableTexture => TableTextureMode.Metal;

        static SemiTruckTankerObject()
        {
            AddOccupancy<SemiTruckTankerObject>(new List<BlockOccupancy>(0));
        }

        private static readonly string[] fuelTagList = new string[]
        {

            "Liquid Fuel"
        };

        private SemiTruckTankerObject() { }

        protected override void Initialize()
        {
            base.Initialize();
            GetComponent<MinimapComponent>().InitAsMovable();
            GetComponent<MinimapComponent>().SetCategory(Localizer.DoStr("Vehicles"));
            GetComponent<CustomTextComponent>().Initialize(200);
            GetComponent<FuelSupplyComponent>().Initialize(4, fuelTagList);
            GetComponent<FuelConsumptionComponent>().Initialize(40);
            GetComponent<AirPollutionComponent>().Initialize(0.5f);
            GetComponent<VehicleComponent>().Initialize(20, 1.8f, 2);
            GetComponent<PublicStorageComponent>().Initialize(50, 2500000, new TagRestriction("Liquid Fuel"));
            GetComponent<StockpileComponent>().Initialize(new Vector3i(2, 2, 3));
            GetComponent<PartsComponent>().Config(() => LocString.Empty, new PartInfo[]
            {
                new() { TypeName = nameof(CombustionEngineItem), Quantity = 1},
                new() { TypeName = nameof(RubberWheelItem), Quantity = 4},
                new() { TypeName = nameof(LightBulbItem), Quantity = 1},
                new() { TypeName = nameof(LubricantItem), Quantity = 2},
            }); 
        }
    }
}
