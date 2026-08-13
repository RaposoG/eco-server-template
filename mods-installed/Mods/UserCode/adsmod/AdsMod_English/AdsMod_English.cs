// MOD created by Plex: 3D Model and Code.
// Last mod update: 04/19/26

// Some storage settings can be modified. Annotations are available to indicate possible modifications.
// Please do not remove the "Registered Mod" section from the code, as it allows you to receive compensation from Strange Loop Games when used on an online server.


namespace Eco.Mods.TechTree
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Eco.Core.Items;
    using Eco.Gameplay.Blocks;
    using Eco.Gameplay.Components;
    using Eco.Gameplay.Components.Auth;
    using Eco.Gameplay.DynamicValues;
    using Eco.Gameplay.Economy;
    using Eco.Gameplay.Housing;
    using Eco.Gameplay.Interactions;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Modules;
    using Eco.Gameplay.Minimap;
    using Eco.Gameplay.Objects;
    using Eco.Gameplay.Occupancy;
    using Eco.Gameplay.Players;
    using Eco.Gameplay.Property;
    using Eco.Gameplay.Skills;
    using Eco.Gameplay.Systems;
    using Eco.Gameplay.Systems.TextLinks;
    using Eco.Gameplay.Pipes.LiquidComponents;
    using Eco.Gameplay.Pipes.Gases;
    using Eco.Shared;
    using Eco.Shared.Math;
    using Eco.Shared.Localization;
    using Eco.Shared.Serialization;
    using Eco.Shared.Utils;
    using Eco.Shared.View;
    using Eco.Shared.Items;
    using Eco.Shared.Networking;
    using Eco.Gameplay.Pipes;
    using Eco.World.Blocks;
    using Eco.Gameplay.Housing.PropertyValues;
    using Eco.Gameplay.Civics.Objects;
    using Eco.Gameplay.Settlements;
    using Eco.Gameplay.Systems.NewTooltip;
    using Eco.Core.Controller;
    using Eco.Core.Utils;
    using Eco.Gameplay.Components.Storage;
    using static Eco.Gameplay.Housing.PropertyValues.HomeFurnishingValue;
    using Eco.Gameplay.Items.Recipes;
    using Eco.Core.Plugins.Interfaces;

    #region ModRegistration
    public class AdsMod : IModInit
    {
        public static ModRegistration Register() => new()
        {
            ModName = "AdsMod",
            ModDescription = "AdsMod ajoute au jeu trois panneaux publicitaires, offrant aux joueurs la possibilité d’y afficher l’image de leur choix grâce au système d’imprimante.",
            ModDisplayName = "Ads Mod",
        };
    }
    #endregion


    #region Pub01
    [Serialized]
    [RequireComponent(typeof(PropertyAuthComponent))]
    [RequireComponent(typeof(OccupancyRequirementComponent))]
    [RequireComponent(typeof(ForSaleComponent))]
    [Tag("Usable")]
    [Ecopedia("Housing Objects", "Outdoor", subPageName: "Large Format Panel")]
    public partial class affiche_pub01Object : PictureFrameObject, IRepresentsItem
    {
        public virtual Type RepresentedItemType => typeof(affiche_pub01Item);
        public override LocString DisplayName => Localizer.DoStr("Large Format Panel");
        public override TableTextureMode TableTexture => TableTextureMode.Metal;

        protected override void Initialize()
        {
            this.ModsPreInitialize();
            base.Initialize();

            this.ModsPostInitialize();
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }

    [Serialized]
    [LocDisplayName("Large Format Panel")]
    [LocDescription("A large panel to display your messages or advertising campaigns all over the planet !")]
    [Ecopedia("Housing Objects", "Outdoor", createAsSubPage: true)]
    [Tag("Housing")]
    [Weight(1000)]
    public partial class affiche_pub01Item : WorldObjectItem<affiche_pub01Object>
    {
        protected override OccupancyContext GetOccupancyContext => new SideAttachedContext(0 | DirectionAxisFlags.Down, WorldObject.GetOccupancyInfo(this.WorldObjectType));

    }

    [RequiresSkill(typeof(BlacksmithSkill), 5)]
    [Ecopedia("Housing Objects", "Outdoor", createAsSubPage: true)]
    public partial class affiche_pub01Recipe : RecipeFamily
    {
        public affiche_pub01Recipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Large Format Panel",
                displayName: Localizer.DoStr("Large Format Panel"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(IronBarItem), 20, typeof(BlacksmithSkill)),
                    new IngredientElement(typeof(IronPlateItem), 30, typeof(BlacksmithSkill)),
                    new IngredientElement(typeof(GlassItem), 10, typeof(BlacksmithSkill)),
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<affiche_pub01Item>()
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 0.1f;

            this.LaborInCalories = CreateLaborInCaloriesValue(350, typeof(BlacksmithSkill));
            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(affiche_pub01Recipe), start: 0.5f, skillType: typeof(BlacksmithSkill));

            this.ModsPreInitialize();
            this.Initialize(displayText: Localizer.DoStr("Large Format Panel"), recipeType: typeof(affiche_pub01Recipe));
            this.ModsPostInitialize();

            CraftingComponent.AddRecipe(tableType: typeof(BlacksmithTableObject), recipeFamily: this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
    #endregion



    #region Pub02
    [Serialized]
    [RequireComponent(typeof(PropertyAuthComponent))]
    [RequireComponent(typeof(OccupancyRequirementComponent))]
    [RequireComponent(typeof(ForSaleComponent))]
    [Tag("Usable")]
    [Ecopedia("Housing Objects", "Outdoor", subPageName: "Urban Sign")]
    public partial class affiche_pub02Object : PictureFrameObject, IRepresentsItem
    {
        public virtual Type RepresentedItemType => typeof(affiche_pub02Item);
        public override LocString DisplayName => Localizer.DoStr("Urban Sign");
        public override TableTextureMode TableTexture => TableTextureMode.Metal;

        protected override void Initialize()
        {
            this.ModsPreInitialize();
            base.Initialize();

            this.ModsPostInitialize();
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }

    [Serialized]
    [LocDisplayName("Urban Sign")]
    [LocDescription("A modern billboard for broadcasting announcements or decorating cities.")]
    [Ecopedia("Housing Objects", "Outdoor", createAsSubPage: true)]
    [Tag("Housing")]
    [Weight(1000)] 
    public partial class affiche_pub02Item : WorldObjectItem<affiche_pub02Object>
    {
        protected override OccupancyContext GetOccupancyContext => new SideAttachedContext(0 | DirectionAxisFlags.Down, WorldObject.GetOccupancyInfo(this.WorldObjectType));

    }

    [RequiresSkill(typeof(BlacksmithSkill), 3)]
    [Ecopedia("Housing Objects", "Outdoor", createAsSubPage: true)]
    public partial class affiche_pub02Recipe : RecipeFamily
    {
        public affiche_pub02Recipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Urban Sign",  
                displayName: Localizer.DoStr("Urban Sign"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(IronBarItem), 10, typeof(BlacksmithSkill)),
                    new IngredientElement(typeof(IronPlateItem), 15, typeof(BlacksmithSkill)),
                    new IngredientElement(typeof(GlassItem), 5, typeof(BlacksmithSkill)),
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<affiche_pub02Item>()
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 0.1f;

            this.LaborInCalories = CreateLaborInCaloriesValue(250, typeof(BlacksmithSkill));
            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(affiche_pub02Recipe), start: 0.5f, skillType: typeof(BlacksmithSkill));

            this.ModsPreInitialize();
            this.Initialize(displayText: Localizer.DoStr("Urban Sign"), recipeType: typeof(affiche_pub02Recipe));
            this.ModsPostInitialize();

            CraftingComponent.AddRecipe(tableType: typeof(BlacksmithTableObject), recipeFamily: this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
    #endregion



    #region Pub03
    [Serialized]
    [RequireComponent(typeof(PropertyAuthComponent))]
    [RequireComponent(typeof(OccupancyRequirementComponent))]
    [RequireComponent(typeof(ForSaleComponent))]
    [Tag("Usable")]
    [Ecopedia("Housing Objects", "Outdoor", subPageName: "Mobile Poster")]
    public partial class affiche_pub04Object : PictureFrameObject, IRepresentsItem
    {
        public virtual Type RepresentedItemType => typeof(affiche_pub04Item);
        public override LocString DisplayName => Localizer.DoStr("Mobile Poster");
        public override TableTextureMode TableTexture => TableTextureMode.Metal;

        protected override void Initialize()
        {
            this.ModsPreInitialize();
            base.Initialize();

            this.ModsPostInitialize();
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }

    [Serialized]
    [LocDisplayName("Mobile Poster")]
    [LocDescription("An easy-to-move standing advertisement to promote your businesses in your towns.")]
    [Ecopedia("Housing Objects", "Outdoor", createAsSubPage: true)]
    [Tag("Housing")]
    [Weight(1000)] 
    public partial class affiche_pub04Item : WorldObjectItem<affiche_pub04Object>
    {
        protected override OccupancyContext GetOccupancyContext => new SideAttachedContext(0 | DirectionAxisFlags.Down, WorldObject.GetOccupancyInfo(this.WorldObjectType));

    }

    [RequiresSkill(typeof(BlacksmithSkill), 1)]
    [Ecopedia("Housing Objects", "Outdoor", createAsSubPage: true)]
    public partial class affiche_pub04Recipe : RecipeFamily
    {
        public affiche_pub04Recipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Mobile Poster",
                displayName: Localizer.DoStr("Mobile Poster"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(IronBarItem), 5, typeof(BlacksmithSkill)),
                    new IngredientElement(typeof(IronPlateItem), 10, typeof(BlacksmithSkill)),
                    new IngredientElement(typeof(GlassItem), 4, typeof(BlacksmithSkill)),
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<affiche_pub04Item>()
                });
            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 0.1f;

            this.LaborInCalories = CreateLaborInCaloriesValue(100, typeof(BlacksmithSkill));
            this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(affiche_pub04Recipe), start: 0.5f, skillType: typeof(BlacksmithSkill));

            this.ModsPreInitialize();
            this.Initialize(displayText: Localizer.DoStr("Mobile Poster"), recipeType: typeof(affiche_pub04Recipe));
            this.ModsPostInitialize();

            CraftingComponent.AddRecipe(tableType: typeof(BlacksmithTableObject), recipeFamily: this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
    #endregion

}