
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

    [RequiresSkill(typeof(AnimalHusbandrySkill), 3)] 
    public partial class BreedCowsRecipe :
        RecipeFamily
    {
        public BreedCowsRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "BreedCow",  //noloc
                displayName: Localizer.DoStr("Breed Cow"),

                // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                // type of the item, the amount of the item, the skill required, and the talent used.
                ingredients: new List<IngredientElement>
                {
               new IngredientElement(typeof(CowsItem), 2, true),
               new IngredientElement(typeof(HerbivoreRationItem), 4, typeof(AnimalHusbandrySkill), typeof(AnimalHusbandryLavishResourcesTalent)), 
                },
                garbages: new List<GarbageOutput>
                {
                },
                // Define our recipe output items.
                // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                // to create.
                items: new List<CraftingElement>
                {
               new CraftingElement<CowsItem>(3)
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 5;  
            this.LaborInCalories = CreateLaborInCaloriesValue(150, typeof(AnimalHusbandrySkill)); 
            this.CraftMinutes = CreateCraftTimeValue(typeof(BreedCowsRecipe), 30, typeof(AnimalHusbandrySkill), typeof(AnimalHusbandryFocusedSpeedTalent), typeof(AnimalHusbandryParallelSpeedTalent));     
            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Breed Cow"), typeof(BreedCowsRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(AnimalFeederObject), this);
        }

        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();
        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();
    }
}
