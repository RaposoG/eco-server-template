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
using Eco.Gameplay.Items.Recipes;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Occupancy;
using Eco.Shared.Logging;
using static Eco.Gameplay.Components.PartsComponent;

namespace Eco.EM.Machines.Trucking
{
    [Serialized]
    [LocDisplayName("Semi Truck - Logging Trailer")]
    [Weight(25000)]
    [AirPollution(0.9f)]
    [Ecopedia("Crafted Objects", "Vehicles", createAsSubPage: true)]
    [LocDescription("Modern Truck With a Semi Trailer attached for transporting Logs")]
    public partial class SemiTruckLoggingItem : WorldObjectItem<SemiTruckLoggingObject>, IPersistentData
    {
        [Serialized, SyncToView, NewTooltipChildren(CacheAs.Instance, flags: TTFlags.AllowNonControllerTypeForChildren)] public object PersistentData { get; set; }
    }

    [ForceCreateView]
    public partial class SemiTruckLoggingRecipe : Recipe
    {
        public SemiTruckLoggingRecipe()
        {
            this.Init(
                name: "Semi Truck - Logging Trailer",  //noloc
                    displayName: Localizer.DoStr("Semi Truck - Logging Trailer"),

                    // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                    // type of the item, the amount of the item, the skill required, and the talent used.
                    ingredients: new List<IngredientElement>
                    {
                    new IngredientElement(typeof(LoggingTrailerItem), 1, true),
                    new IngredientElement(typeof(PrimeMoverItem), 1, true),
                    },

                    // Define our recipe output items.
                    // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                    // to create.
                    items: new List<CraftingElement>
                    {
                    new CraftingElement<SemiTruckLoggingItem>()
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

    public partial class SemiTruckLoggingObject : PhysicsWorldObject, IRepresentsItem
    {
        public override LocString DisplayName => Localizer.DoStr("Logging Truck");
        public Type RepresentedItemType => typeof(SemiTruckLoggingItem);
        public override TableTextureMode TableTexture => TableTextureMode.Metal;

        public override float InteractDistance => 16f;

        static SemiTruckLoggingObject()
        {
            AddOccupancy<SemiTruckLoggingObject>(new List<BlockOccupancy>(0));
        }
        private static readonly string[] fuelTagList = new string[]
        {

            "Liquid Fuel"
        };

        private SemiTruckLoggingObject() { }

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
            GetComponent<PublicStorageComponent>().Initialize(70, 100000000, new TagStackRestriction(2, "Wood", "Lumber", "Hewn", "Composites"));
            GetComponent<StockpileComponent>().Initialize(new Vector3i(3, 3, 13));
            GetComponent<PartsComponent>().Config(() => LocString.Empty, new PartInfo[]
            {
                new() { TypeName = nameof(CombustionEngineItem), Quantity = 1},
                new() { TypeName = nameof(RubberWheelItem), Quantity = 4},
                new() { TypeName = nameof(LightBulbItem), Quantity = 1},
                new() { TypeName = nameof(LubricantItem), Quantity = 2},
            });
        }
    }
    public class TagStackRestriction : InventoryRestriction
    {
        private List<string> allowedTags;
        private float Multiplier { get; set; } = 1;

        public TagStackRestriction(float multiplier, params string[] allowedTags)
        {
            this.allowedTags = new List<string>(allowedTags);
            Multiplier = multiplier;
        }
        public TagStackRestriction(float multiplier, IEnumerable<Tag> allowedTags)
        {
            this.allowedTags = new List<string>(allowedTags.Select(tag => tag.Name));
            Multiplier = multiplier;
        }

        public override LocString Message => Localizer.DoStr("Inventory does not accept that type of item.");
        public override int MaxAccepted(Item item, int currentQuantity) => item.Tags().Any(x => this.allowedTags.Contains(x.Name)) ? (int)(item.MaxStackSize * Multiplier) : 0;
    }
}