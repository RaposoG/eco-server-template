using System;
using System.Collections.Generic;
using Eco.Gameplay.Components;
using Eco.Gameplay.Components.Auth;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Skills;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Serialization;
//using Eco.EM.Framework.Resolvers;
using System.Linq;
using Eco.Gameplay.Items.Recipes;
using Eco.Gameplay.Occupancy;
using Eco.Core.Controller;

namespace Eco.EM.Machines.Trucking
{
    [Serialized, Weight(200), MaxStackSize(50), LocDisplayName("Car Carrier Trailer")]
    [LocDescription("Car Carrier Trailer")]
    public partial class PlantTrailerItem : Item
    {

    }

    [ForceCreateView]
    [RequiresSkill(typeof(IndustrySkill), 7)]
    public partial class PlantTrailerRecipe : RecipeFamily//, IConfigurableRecipe
    {
        public PlantTrailerRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Car Carrier Trailer",  //noloc
                displayName: Localizer.DoStr("Car Carrier Trailer"),

                // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                // type of the item, the amount of the item, the skill required, and the talent used.
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(RubberWheelItem), 6, true),
                    new IngredientElement(typeof(SteelAxleItem), 3, true),
                    new IngredientElement(typeof(LightBulbItem), 4, true),
                    new IngredientElement(typeof(LubricantItem), 2, true),
                },

                // Define our recipe output items.
                // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                // to create.
                items: new List<CraftingElement>
                {
                    new CraftingElement<PlantTrailerItem>()
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 18; // Defines how much experience is gained when crafted.

            // Defines the amount of labor required and the required skill to add labor
            this.LaborInCalories = CreateLaborInCaloriesValue(2000, typeof(IndustrySkill));

            // Defines our crafting time for the recipe
            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(PlantTrailerRecipe), start: 10, skillType: typeof(IndustrySkill));

            // Perform pre/post initialization for user mods and initialize our recipe instance with the display name "Truck"
            this.ModsPreInitialize();
            this.Initialize(displayText: Localizer.DoStr("Car Carrier Trailer"), recipeType: typeof(PlantTrailerRecipe));
            this.ModsPostInitialize();

            // Register our RecipeFamily instance with the crafting system so it can be crafted.
            CraftingComponent.AddRecipe(tableType: typeof(RoboticAssemblyLineObject), this);
        }

        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();

        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();

        //static RecipeDefaultModel Defaults => new()
        //{
        //    ModelType = typeof(PlantTrailerRecipe).Name,
        //    Assembly = typeof(PlantTrailerRecipe).AssemblyQualifiedName,
        //    HiddenName = "Logistics Trailer",
        //    LocalizableName = Localizer.DoStr("Logistics Trailer"),
        //    IngredientList =
        //    [
        //        new EMIngredient("SteelPlateItem", false, 120),
        //        new EMIngredient("RivetItem", false, 80),
        //        new EMIngredient("RubberWheelItem", false, 8, true),
        //        new EMIngredient("SteelAxleItem", false, 3, true),
        //
        //    ],
        //    ProductList =
        //    [
        //        new EMCraftable("PlantTrailerItem"),
        //    ],
        //    BaseExperienceOnCraft = 1f,
        //    BaseLabor = 2000,
        //    LaborIsStatic = false,
        //    BaseCraftTime = 5,
        //    CraftTimeIsStatic = false,
        //    CraftingStation = "RoboticAssemblyLineItem",
        //    RequiredSkillType = typeof(IndustrySkill),
        //    RequiredSkillLevel = 7,
        //    SpeedImprovementTalents = [typeof(IndustryFocusedSpeedTalent), typeof(IndustryParallelSpeedTalent)]
        //};
        //
        //static PlantTrailerRecipe() { EMRecipeResolver.AddDefaults(Defaults); }
        //
        //public PlantTrailerRecipe()
        //{
        //    this.Recipes = EMRecipeResolver.Obj.ResolveRecipe(this);
        //    this.LaborInCalories = EMRecipeResolver.Obj.ResolveLabor(this);
        //    this.CraftMinutes = EMRecipeResolver.Obj.ResolveCraftMinutes(this);
        //    this.ExperienceOnCraft = EMRecipeResolver.Obj.ResolveExperience(this);
        //    this.Initialize(EMRecipeResolver.Obj.ResolveRecipeName(this), GetType());
        //    CraftingComponent.AddRecipe(EMRecipeResolver.Obj.ResolveStation(this), this);
        //}
    }
}
