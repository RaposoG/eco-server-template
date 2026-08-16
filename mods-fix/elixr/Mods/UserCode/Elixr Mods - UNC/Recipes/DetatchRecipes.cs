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
using System.Linq;
using Eco.Gameplay.Items.Recipes;
using Eco.Gameplay.Occupancy;
using Eco.Core.Controller;
using Eco.Shared.Logging;

namespace Eco.EM.Machines.Trucking
{
    public partial class DetatchSemiTrailerRecipe : RecipeFamily
    {
        public DetatchSemiTrailerRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "DetatchSemiTrailer",
                displayName: Localizer.DoStr("Detatch Semi Trailer"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(SemiTruckItem), 1, true),
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<SemiTrailerItem>(),
                    new CraftingElement<PrimeMoverItem>()
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 0;
            this.CraftMinutes = CreateCraftTimeValue(0f);
            this.LaborInCalories = CreateLaborInCaloriesValue(0);

            this.ModsPreInitialize();
            this.Initialize(displayText: Localizer.DoStr("Detatch Semi Trailer"), recipeType: typeof(DetatchSemiTrailerRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(tableType: typeof(AssemblyLineObject), recipeFamily: this);
        }

        partial void ModsPreInitialize();

        partial void ModsPostInitialize();

    }

    [ForceCreateView]
    public partial class DetatchTankerTrailerRecipe : Recipe
    {
        public DetatchTankerTrailerRecipe()
        {
            this.Init(
                name: "Detatch Tanker Trailer",  //noloc
                    displayName: Localizer.DoStr("Detatch Tanker Trailer"),

                    ingredients: new List<IngredientElement>
                    {
                    new IngredientElement(typeof(SemiTruckTankerItem), 1, true),
                    
                    },

                    items: new List<CraftingElement>
                    {
                    new CraftingElement<TankerTrailerItem>(),
                    new CraftingElement<PrimeMoverItem>()

                });

            this.ModsPostInitialize();

            CraftingComponent.AddTagProduct(typeof(RoboticAssemblyLineObject), typeof(DetatchSemiTrailerRecipe), this);

        }

        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();

        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();
    }
    [ForceCreateView]
    public partial class DetatchMiningTrailerRecipe : Recipe
    {
        public DetatchMiningTrailerRecipe()
        {
            this.Init(
                name: "Detatch Mining Trailer",  //noloc
                    displayName: Localizer.DoStr("Detatch Mining Trailer"),

                    // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                    // type of the item, the amount of the item, the skill required, and the talent used.
                    ingredients: new List<IngredientElement>
                    {
                    new IngredientElement(typeof(SemiTruckMiningItem), 1, true),

                    },

                    // Define our recipe output items.
                    // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                    // to create.
                    items: new List<CraftingElement>
                    {
                    new CraftingElement<MiningTrailerItem>(),
                    new CraftingElement<PrimeMoverItem>()

                });

            // Perform pre/post initialization for user mods and initialize our recipe instance with the display name "Truck"
            this.ModsPostInitialize();

            // Register our RecipeFamily instance with the crafting system so it can be crafted.
            CraftingComponent.AddTagProduct(typeof(RoboticAssemblyLineObject), typeof(DetatchSemiTrailerRecipe), this);

        }
        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();

        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();
    }
    [ForceCreateView]
    public partial class DetatchLoggingTrailerRecipe : Recipe
    {
        public DetatchLoggingTrailerRecipe()
        {
            this.Init(
                name: "Detatch Logging Trailer",  //noloc
                    displayName: Localizer.DoStr("Detatch Logging Trailer"),

                    // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                    // type of the item, the amount of the item, the skill required, and the talent used.
                    ingredients: new List<IngredientElement>
                    {
                    new IngredientElement(typeof(SemiTruckLoggingItem), 1, true),

                    },

                    // Define our recipe output items.
                    // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                    // to create.
                    items: new List<CraftingElement>
                    {
                    new CraftingElement<LoggingTrailerItem>(),
                    new CraftingElement<PrimeMoverItem>()

                });

            // Perform pre/post initialization for user mods and initialize our recipe instance with the display name "Truck"
            this.ModsPostInitialize();

            // Register our RecipeFamily instance with the crafting system so it can be crafted.
            CraftingComponent.AddTagProduct(typeof(RoboticAssemblyLineObject), typeof(DetatchSemiTrailerRecipe), this);

        }
        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();

        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();
    }
    [ForceCreateView]
    public partial class DetatchFridgeTrailerRecipe : Recipe
    {
        public DetatchFridgeTrailerRecipe()
        {
            this.Init(
                name: "Detatch Fridge Trailer",  //noloc
                    displayName: Localizer.DoStr("Detatch Fridge Trailer"),

                    // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                    // type of the item, the amount of the item, the skill required, and the talent used.
                    ingredients: new List<IngredientElement>
                    {
                    new IngredientElement(typeof(SemiTruckFridgeItem), 1, true),

                    },

                    // Define our recipe output items.
                    // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                    // to create.
                    items: new List<CraftingElement>
                    {
                    new CraftingElement<FridgeTrailerItem>(),
                    new CraftingElement<PrimeMoverItem>()

                });

            // Perform pre/post initialization for user mods and initialize our recipe instance with the display name "Truck"
            this.ModsPostInitialize();

            // Register our RecipeFamily instance with the crafting system so it can be crafted.
            CraftingComponent.AddTagProduct(typeof(RoboticAssemblyLineObject), typeof(DetatchSemiTrailerRecipe), this);

        }
        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();

        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();
    }
    [ForceCreateView]
    public partial class DetatchPlantTrailerRecipe : Recipe
    {
        public DetatchPlantTrailerRecipe()
        {
            this.Init(
                name: "Detatch Car Carrier Trailer",  //noloc
                    displayName: Localizer.DoStr("Detatch Car Carrier Trailer"),

                    // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                    // type of the item, the amount of the item, the skill required, and the talent used.
                    ingredients: new List<IngredientElement>
                    {
                    new IngredientElement(typeof(SemiTruckPlantItem), 1, true),

                    },

                    // Define our recipe output items.
                    // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                    // to create.
                    items: new List<CraftingElement>
                    {
                    new CraftingElement<PlantTrailerItem>(),
                    new CraftingElement<PrimeMoverItem>()

                });

            this.ModsPostInitialize();

            // Register our RecipeFamily instance with the crafting system so it can be crafted.
            CraftingComponent.AddTagProduct(typeof(RoboticAssemblyLineObject), typeof(DetatchSemiTrailerRecipe), this);

        }
        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();

        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();
    }
}
