// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

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
    using Eco.Gameplay.Housing.PropertyValues;

    /// <summary>
    /// <para>Plain (non-seed) item used only as the Hydrotable/Crop Greenhouse ingredient for growing Rice.</para>
    /// <para>Not a SeedItem: cannot be planted in the world, only crafted and consumed at Hydrotable/Crop Greenhouse recipes.</para>
    /// </summary>
    [Serialized]
    [LocDisplayName("Rice Culture")]
    [Weight(50)]
    [Ecopedia("Food", "Seed", subPageName: "Rice Culture Item")]
    [LocDescription("A sterile rice culture propagated from rice grain, used to start new rice growth in a Hydrotable or Crop Greenhouse. Cannot be planted in the ground.")]
    public partial class RiceCultureItem : Item
    {
    }

    /// <summary>
    /// <para>Server side recipe definition for "RiceCulture".</para>
    /// </summary>
    [RequiresSkill(typeof(FarmingSkill), 1)]
    public partial class RiceCultureRecipe : RecipeFamily
    {
        public RiceCultureRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Rice Culture",  //noloc
                displayName: Localizer.DoStr("Rice Culture"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(RiceItem), 2, typeof(FarmingSkill)),
                },
                garbages: new List<GarbageOutput>
                {
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<RiceCultureItem>(6)
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 1; // Defines how much experience is gained when crafted.

            // Defines the amount of labor required and the required skill to add labor
            this.LaborInCalories = CreateLaborInCaloriesValue(60, typeof(FarmingSkill));

            // Defines our crafting time for the recipe
            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(RiceCultureRecipe), start: 0.2f, skillType: typeof(FarmingSkill));

            this.ModsPreInitialize();
            this.Initialize(displayText: Localizer.DoStr("Rice Culture"), recipeType: typeof(RiceCultureRecipe));
            this.ModsPostInitialize();

            // Register our RecipeFamily instance with the crafting system so it can be crafted.
            CraftingComponent.AddRecipe(tableType: typeof(FarmersTableObject), recipeFamily: this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
