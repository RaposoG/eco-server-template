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
using Eco.Gameplay.Items.Recipes;
using Eco.Gameplay.Occupancy;

namespace Eco.EM.Machines.Trucking
{
    [Serialized, Weight(200), MaxStackSize(50), LocDisplayName("Tanker Trailer")]
    [LocDescription("Tanker Transport Trailer")]
    public partial class TankerTrailerItem : Item
    {
        
    }

    [RequiresSkill(typeof(IndustrySkill), 4)]
    public partial class TankerTrailerRecipe : RecipeFamily//, IConfigurableRecipe
    {
        public TankerTrailerRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Tanker Trailer",  //noloc
                displayName: Localizer.DoStr("Tanker Trailer"),

                // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                // type of the item, the amount of the item, the skill required, and the talent used.
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(RubberWheelItem), 6, true),
                    new IngredientElement(typeof(RadiatorItem), 1, true),
                    new IngredientElement(typeof(SteelAxleItem), 3, true),
                    new IngredientElement(typeof(LightBulbItem), 4, true),
                    new IngredientElement(typeof(LubricantItem), 2, true),
                },

                // Define our recipe output items.
                // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                // to create.
                items: new List<CraftingElement>
                {
                    new CraftingElement<TankerTrailerItem>()
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 18; // Defines how much experience is gained when crafted.

            // Defines the amount of labor required and the required skill to add labor
            this.LaborInCalories = CreateLaborInCaloriesValue(2000, typeof(IndustrySkill));

            // Defines our crafting time for the recipe
            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(TankerTrailerRecipe), start: 10, skillType: typeof(IndustrySkill));

            // Perform pre/post initialization for user mods and initialize our recipe instance with the display name "Truck"
            this.ModsPreInitialize();
            this.Initialize(displayText: Localizer.DoStr("Tanker Trailer"), recipeType: typeof(TankerTrailerRecipe));
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
        //    ModelType = typeof(TankerTrailerRecipe).Name,
        //    Assembly = typeof(TankerTrailerRecipe).AssemblyQualifiedName,
        //    HiddenName = "Tanker Trailer",
        //    LocalizableName = Localizer.DoStr("Tanker Trailer"),
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
        //        new EMCraftable("TankerTrailerItem"),
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
        //static TankerTrailerRecipe() { EMRecipeResolver.AddDefaults(Defaults); }
        //
        //public TankerTrailerRecipe()
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
