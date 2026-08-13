

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
    public partial class ButcherGMSheepRecipe :
        RecipeFamily
    {
        public ButcherGMSheepRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "ButcherGMSheep",  //noloc
                displayName: Localizer.DoStr("Butcher GM Sheep"),

                // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                // type of the item, the amount of the item, the skill required, and the talent used.
                ingredients: new List<IngredientElement>
                {
               new IngredientElement(typeof(GMSheepItem), 1, typeof(ButcherySkill)),    
                },
                garbages: new List<GarbageOutput>
                {
                },
                // Define our recipe output items.
                // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                // to create.
                items: new List<CraftingElement>
                {
               new CraftingElement<RawMeatItem>(7), 
               new CraftingElement<LeatherHideItem>(2), 
               new CraftingElement<ShornWoolItem>(8),
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 4;  
            this.LaborInCalories = CreateLaborInCaloriesValue(80, typeof(ButcherySkill)); 
            this.CraftMinutes = CreateCraftTimeValue(typeof(ButcherGMSheepRecipe), 1.5f, typeof(ButcherySkill));     
            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Butcher GM Sheep"), typeof(ButcherGMSheepRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(ButcheryTableObject), this);
        }

        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();
        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();
    }
}
