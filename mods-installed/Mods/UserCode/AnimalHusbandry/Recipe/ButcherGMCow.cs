
namespace Eco.Mods.TechTree
{
        using System;
    using System.Collections.Generic;
    using Eco.Gameplay.Components;
    using Eco.Gameplay.DynamicValues;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Players;
    using Eco.Gameplay.Skills;
    using Eco.Shared.Utils;
    using Eco.World;
    using Eco.World.Blocks;
    using Gameplay.Systems.TextLinks;
    using Eco.Shared.Localization;
    using Eco.Gameplay.Items.Recipes;
    using Eco.Gameplay.Garbage;

    [RequiresSkill(typeof(ButcherySkill), 6)] 
    public partial class ButcherGMCowRecipe :
        RecipeFamily
    {
        public ButcherGMCowRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "ButcherGMCow",  //noloc
                displayName: Localizer.DoStr("Butcher GM Cow"),

                // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                // type of the item, the amount of the item, the skill required, and the talent used.
                ingredients: new List<IngredientElement>
                {
               new IngredientElement(typeof(GMCowItem), 1, typeof(ButcherySkill)),  
                },
                garbages: new List<GarbageOutput>
                {
                },
                // Define our recipe output items.
                // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                // to create.
                items: new List<CraftingElement>
                {
               new CraftingElement<RawMeatItem>(12), 
               new CraftingElement<LeatherHideItem>(3)
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 5;  
            this.LaborInCalories = CreateLaborInCaloriesValue(120, typeof(ButcherySkill)); 
            this.CraftMinutes = CreateCraftTimeValue(typeof(ButcherGMCowRecipe), 2, typeof(ButcherySkill));     
            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Butcher GM Cow"), typeof(ButcherGMCowRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(ButcheryTableObject), this);
        }

        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();
        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();
    }
}
