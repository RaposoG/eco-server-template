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
    [LocDisplayName("Semi Truck - Car Carrier")]
    [Weight(25000)]
    [AirPollution(0.5f)]
    [Ecopedia("Crafted Objects", "Vehicles", createAsSubPage: true)]
    [LocDescription("Modern Truck With a Semi Trailer attached for transporting Vehicles")]
    public partial class SemiTruckPlantItem : WorldObjectItem<SemiTruckPlantObject>, IPersistentData
    {
        [Serialized, SyncToView, NewTooltipChildren(CacheAs.Instance, flags: TTFlags.AllowNonControllerTypeForChildren)] public object PersistentData { get; set; }
    }

    [ForceCreateView]
    public partial class SemiTruckPlantRecipe : Recipe
    {
        public SemiTruckPlantRecipe()
        {
            this.Init(
                name: "Semi Truck - Plant Trailer",  //noloc
                    displayName: Localizer.DoStr("Semi Truck - Plant Trailer"),

                    // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                    // type of the item, the amount of the item, the skill required, and the talent used.
                    ingredients: new List<IngredientElement>
                    {
                    new IngredientElement(typeof(PlantTrailerItem), 1, true),
                    new IngredientElement(typeof(PrimeMoverItem), 1, true),
                    },

                    // Define our recipe output items.
                    // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                    // to create.
                    items: new List<CraftingElement>
                    {
                    new CraftingElement<SemiTruckPlantItem>()
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
    public partial class SemiTruckPlantObject : PhysicsWorldObject, IRepresentsItem
    {
        public override LocString DisplayName => Localizer.DoStr("Semi Truck - Car Carrier");
        public Type RepresentedItemType => typeof(SemiTruckPlantItem);

        public override float InteractDistance => 16f;
        public override TableTextureMode TableTexture => TableTextureMode.Metal;


        static SemiTruckPlantObject()
        {
            AddOccupancy<SemiTruckPlantObject>(new List<BlockOccupancy>(0));
        }

        private static readonly string[] fuelTagList = new string[]
        {

            "Liquid Fuel"
        };

        private SemiTruckPlantObject() { }

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
            GetComponent<PublicStorageComponent>().Initialize(10, 2500000);
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

    public class VehicleItemsStackRestriction : InventoryRestriction
    {
        private readonly HashSet<Type> allowedItemTypes = new(new Type[] { typeof(PhysicsWorldObject) });
        public VehicleItemsStackRestriction() { }

        public override LocString Message => Localizer.DoStr("Plant Trailer can only Store Small Vehicles");

        public override int MaxAccepted(Item item, int currentQuantity)
        {
            if (allowedItemTypes.Contains(item.GetType())) return -1;
            else
                return 0;
        }
    }
}
