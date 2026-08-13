
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

    [RequiresSkill(typeof(ButcherySkill), 1)] 
    public partial class HerbivoreLureRecipe :
        RecipeFamily
    {
        public HerbivoreLureRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "HerbivoreLure",  //noloc
                displayName: Localizer.DoStr("Herbivore Lure"),

                // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                // type of the item, the amount of the item, the skill required, and the talent used.
                ingredients: new List<IngredientElement>
                {
               new IngredientElement(typeof(CornItem), 7, typeof(ButcherySkill)), 
               new IngredientElement(typeof(FiddleheadsItem), 7, typeof(ButcherySkill)), 
               new IngredientElement(typeof(BeetItem), 7, typeof(ButcherySkill)),  
                },
                garbages: new List<GarbageOutput>
                {
                },
                // Define our recipe output items.
                // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                // to create.
                items: new List<CraftingElement>
                {
               new CraftingElement<HerbivoreLureItem>(1), 
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 1;  
            this.LaborInCalories = CreateLaborInCaloriesValue(40, typeof(ButcherySkill)); 
            this.CraftMinutes = CreateCraftTimeValue(typeof(HerbivoreLureRecipe), 2f, typeof(ButcherySkill));   
            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Herbivore Lure"), typeof(HerbivoreLureRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(ButcheryTableObject), this);
        }

        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();
        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();
    }
}
