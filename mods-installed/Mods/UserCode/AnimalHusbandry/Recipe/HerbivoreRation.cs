

namespace Eco.Mods.TechTree
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Eco.Gameplay.Blocks;
    using Eco.Gameplay.Components;
    using Eco.Gameplay.DynamicValues;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Objects;
    using Eco.Gameplay.Players;
    using Eco.Gameplay.Skills;
    using Eco.Gameplay.Settlements;
    using Eco.Gameplay.Systems;
    using Eco.Gameplay.Systems.TextLinks;
    using Eco.Shared.Localization;
    using Eco.Shared.Serialization;
    using Eco.Shared.Utils;
    using Eco.Core.Items;
    using Eco.World;
    using Eco.World.Blocks;
    using Eco.Gameplay.Pipes;
    using Eco.Core.Controller;
    using Eco.Gameplay.Items.Recipes;
    using Eco.Gameplay.Garbage;

    [RequiresSkill(typeof(AnimalHusbandrySkill), 1)] 
    public partial class HerbivoreRationRecipe : RecipeFamily
    {
        public HerbivoreRationRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "HerbivoreRation",  //noloc
                displayName: Localizer.DoStr("Herbivore Ration"),

                // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                // type of the item, the amount of the item, the skill required, and the talent used.
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("Grain", 10, typeof(AnimalHusbandrySkill)),
                    new IngredientElement("Fruit", 3, typeof(AnimalHusbandrySkill)),
                    new IngredientElement(typeof(PlantFibersItem), 15, typeof(AnimalHusbandrySkill)),  
                },
                garbages: new List<GarbageOutput>
                {
                },
                // Define our recipe output items.
                // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                // to create.
                items: new List<CraftingElement>
                {
               new CraftingElement<HerbivoreRationItem>(1),  
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 1;  
            this.LaborInCalories = CreateLaborInCaloriesValue(40, typeof(AnimalHusbandrySkill)); 
            this.CraftMinutes = CreateCraftTimeValue(typeof(HerbivoreRationRecipe), 2, typeof(AnimalHusbandrySkill), typeof(AnimalHusbandryFocusedSpeedTalent), typeof(AnimalHusbandryParallelSpeedTalent));     
            this.ModsPreInitialize();
            this.Initialize(displayText: Localizer.DoStr("Herbivore Ration"), recipeType: typeof(HerbivoreRationRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(AnimalFeederObject), this);
        }

        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();
        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();
    }
}
