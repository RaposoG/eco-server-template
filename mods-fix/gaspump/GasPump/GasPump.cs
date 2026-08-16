// Made By Donand J Trump, Xi Jinping aka. Kenith Pelletier, Ryan Bosse
// Updated for Eco 0.14: Store/Recipe/occupancy APIs moved.

using System;
using System.Linq;
using System.Collections.Generic;
using Eco.Core.Items;
using Eco.Gameplay.Components;
using Eco.Gameplay.Components.Auth;
using Eco.Gameplay.Components.Store;
using Eco.Gameplay.Items;
using Eco.Gameplay.Items.Recipes;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Occupancy;
using Eco.Gameplay.Skills;
using Eco.Shared.Math;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using Eco.Shared.Items;

namespace Eco.Mods.TechTree
{
    [Serialized]
    [Weight(1000)]
    // Attributes must remain in this order: (SharedLinkComponent, StoreComponent)
    [RequireComponent(typeof(SharedLinkComponent))]
    [RequireComponent(typeof(StoreComponent))]
    [RequireComponent(typeof(OccupancyRequirementComponent))]
    [RequireComponent(typeof(PropertyAuthComponent))]
    [RequireComponent(typeof(MinimapComponent))]
    [RequireComponent(typeof(PowerConsumptionComponent))]
    [RequireComponent(typeof(PowerGridComponent))]
    [RequireComponent(typeof(LinkComponent))]
    public partial class GasPumpObject : WorldObject, IRepresentsItem
    {
        public override LocString DisplayName => Localizer.DoStr("Gas Pump");
        public virtual Type RepresentedItemType => typeof(GasPumpItem);

        static GasPumpObject()
        {
            AddOccupancy<GasPumpObject>(new List<BlockOccupancy>()
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
                new BlockOccupancy(new Vector3i(0, 1, 0)),
            });
        }

        protected override void Initialize()
        {
            this.ModsPreInitialize();
            this.GetComponent<MinimapComponent>().SetCategory(Localizer.DoStr("Economy"));
            this.GetComponent<PowerConsumptionComponent>().Initialize(50);
            this.GetComponent<PowerGridComponent>().Initialize(20, new ElectricPower());
            this.GetComponent<LinkComponent>().Initialize(20);
        }

        partial void ModsPreInitialize();
    }

    [Serialized]
    [LocDisplayName("Gas Pump")]
    [LocDescription("Allows the selling of fuel. Range of 20m to storages. Consumes 50w of Electrical Power")]
    [Ecopedia("Work Stations", "Economic", createAsSubPage: true)]
    public partial class GasPumpItem : WorldObjectItem<GasPumpObject>, IPersistentData
    {
        protected override OccupancyContext GetOccupancyContext => new SideAttachedContext(0 | DirectionAxisFlags.Down, WorldObject.GetOccupancyInfo(this.WorldObjectType));
        [Serialized] public object PersistentData { get; set; }
    }

    [RequiresSkill(typeof(IndustrySkill), 1)]
    public partial class GasPumpRecipe : RecipeFamily
    {
        public GasPumpRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Gas Pump",  //noloc
                displayName: Localizer.DoStr("Gas Pump"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(SteelBarItem), 5, typeof(IndustrySkill)),
                    new IngredientElement(typeof(PlasticItem), 20, typeof(IndustrySkill)),
                    new IngredientElement(typeof(ScrewsItem), 6, typeof(IndustrySkill)),
                    new IngredientElement(typeof(BasicCircuitItem), 2, typeof(IndustrySkill)),
                    new IngredientElement(typeof(GlassItem), 2, typeof(IndustrySkill)),
                    new IngredientElement(typeof(SyntheticRubberItem), 2, typeof(IndustrySkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<GasPumpItem>()
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 1;
            this.LaborInCalories = CreateLaborInCaloriesValue(500, typeof(IndustrySkill));
            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(GasPumpRecipe), start: 5, skillType: typeof(IndustrySkill));

            this.Initialize(displayText: Localizer.DoStr("Gas Pump"), recipeType: typeof(GasPumpRecipe));
            CraftingComponent.AddRecipe(tableType: typeof(RoboticAssemblyLineObject), recipeFamily: this);
        }
    }
}
