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
    /// <para>Plain (non-seed) item used only as the Hydrotable/Crop Greenhouse ingredient for growing Camas Bulb.</para>
    /// <para>Not a SeedItem: cannot be planted in the world, only crafted and consumed at Hydrotable/Crop Greenhouse recipes.</para>
    /// </summary>
    [Serialized]
    [LocDisplayName("Camas Bulblet")]
    [Weight(50)]
    [Ecopedia("Food", "Seed", subPageName: "Camas Bulblet Item")]
    [LocDescription("A small offset bulb propagated from a camas bulb, used to start new camas growth in a Hydrotable or Crop Greenhouse. Cannot be planted in the ground.")]
    public partial class CamasBulbletItem : Item
    {
    }

    /// <summary>
    /// <para>Server side recipe definition for "CamasBulblet".</para>
    /// </summary>
    [RequiresSkill(typeof(FarmingSkill), 1)]
    public partial class CamasBulbletRecipe : RecipeFamily
    {
        public CamasBulbletRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Camas Bulblet",  //noloc
                displayName: Localizer.DoStr("Camas Bulblet"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(CamasBulbItem), 2, typeof(FarmingSkill)),
                },
                garbages: new List<GarbageOutput>
                {
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<CamasBulbletItem>(6)
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 1; // Defines how much experience is gained when crafted.

            // Defines the amount of labor required and the required skill to add labor
            this.LaborInCalories = CreateLaborInCaloriesValue(60, typeof(FarmingSkill));

            // Defines our crafting time for the recipe
            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(CamasBulbletRecipe), start: 0.2f, skillType: typeof(FarmingSkill));

            this.ModsPreInitialize();
            this.Initialize(displayText: Localizer.DoStr("Camas Bulblet"), recipeType: typeof(CamasBulbletRecipe));
            this.ModsPostInitialize();

            // Register our RecipeFamily instance with the crafting system so it can be crafted.
            CraftingComponent.AddRecipe(tableType: typeof(FarmersTableObject), recipeFamily: this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
