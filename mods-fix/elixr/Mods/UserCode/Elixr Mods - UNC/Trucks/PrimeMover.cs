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
using Eco.Core.Controller;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Shared.Items;
using Eco.Gameplay.Items.Recipes;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Occupancy;
using static Eco.Gameplay.Components.PartsComponent;

namespace Eco.EM.Machines.Trucking
{
    [Serialized]
    [LocDisplayName("Prime Mover - Flat Nose")]
    [Weight(25000)]
    [AirPollution(0.9f)]
    [Ecopedia("Crafted Objects", "Vehicles", createAsSubPage: true)]
    [LocDescription("Modern Prime Mover, Can be driven as is or combined with a trailer?")]
    public partial class PrimeMoverItem : WorldObjectItem<PrimeMoverObject>, IPersistentData
    {
        [Serialized, SyncToView, NewTooltipChildren(CacheAs.Instance, flags: TTFlags.AllowNonControllerTypeForChildren)] public object PersistentData { get; set; }
    }

    [RequiresSkill(typeof(IndustrySkill), 4)]
    public partial class PrimeMoverRecipe : RecipeFamily
    {
        public PrimeMoverRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Prime Mover - Flat Nose",  //noloc
                displayName: Localizer.DoStr("Prime Mover - Flat Nose"),

                // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                // type of the item, the amount of the item, the skill required, and the talent used.
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(GearboxItem), 4, typeof(IndustrySkill)),
                    new IngredientElement(typeof(SteelPlateItem), 20, typeof(IndustrySkill)),
                    new IngredientElement(typeof(NylonFabricItem), 20, typeof(IndustrySkill)),
                    new IngredientElement(typeof(SteelSpringItem), 6, typeof(IndustrySkill)),
                    new IngredientElement(typeof(CombustionEngineItem), 1, true),
                    new IngredientElement(typeof(RubberWheelItem), 6, true),
                    new IngredientElement(typeof(RadiatorItem), 1, true),
                    new IngredientElement(typeof(SteelAxleItem), 2, true),
                    new IngredientElement(typeof(LightBulbItem), 4, true),
                    new IngredientElement(typeof(LubricantItem), 2, true),
                },

                // Define our recipe output items.
                // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                // to create.
                items: new List<CraftingElement>
                {
                    new CraftingElement<PrimeMoverItem>()
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 18; // Defines how much experience is gained when crafted.

            // Defines the amount of labor required and the required skill to add labor
            this.LaborInCalories = CreateLaborInCaloriesValue(2000, typeof(IndustrySkill));

            // Defines our crafting time for the recipe
            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(PrimeMoverRecipe), start: 10, skillType: typeof(IndustrySkill));

            // Perform pre/post initialization for user mods and initialize our recipe instance with the display name "Truck"
            this.ModsPreInitialize();
            this.Initialize(displayText: Localizer.DoStr("Prime Mover - Flat Nose"), recipeType: typeof(PrimeMoverRecipe));
            this.ModsPostInitialize();

            // Register our RecipeFamily instance with the crafting system so it can be crafted.
            CraftingComponent.AddRecipe(tableType: typeof(RoboticAssemblyLineObject), this);
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
    public partial class PrimeMoverObject : PhysicsWorldObject, IRepresentsItem
    {
        public override LocString DisplayName => Localizer.DoStr("PrimeMover");
        public Type RepresentedItemType => typeof(PrimeMoverItem);

        public override float InteractDistance => 16f;
        public override TableTextureMode TableTexture => TableTextureMode.Metal;

        static PrimeMoverObject()
        {
            AddOccupancy<PrimeMoverObject>(new List<BlockOccupancy>(0));
        }

        private static readonly string[] fuelTagList = new string[]
        {

            "Liquid Fuel"
        };

        private PrimeMoverObject() { }

        protected override void Initialize()
        {
            base.Initialize();
            GetComponent<CustomTextComponent>().Initialize(200);
            GetComponent<PublicStorageComponent>().Initialize(4, 20000);
            GetComponent<FuelSupplyComponent>().Initialize(4, fuelTagList);
            GetComponent<FuelConsumptionComponent>().Initialize(20);
            GetComponent<AirPollutionComponent>().Initialize(0.5f);
            GetComponent<VehicleComponent>().Initialize(20, 1.8f, 2);
            GetComponent<MinimapComponent>().InitAsMovable();
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
