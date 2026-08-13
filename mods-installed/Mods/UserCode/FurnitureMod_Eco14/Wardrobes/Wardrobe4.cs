// Decompiled and migrated from FurnitureMod for Eco 14.x.
// Reconstructed source: original comments/local variable names are not recoverable from a compiled DLL.
using System;
using System.Collections.Generic;
using Eco.Core.Items;
using Eco.Gameplay.Components;
using Eco.Gameplay.Components.Auth;
using Eco.Gameplay.Housing;
using Eco.Gameplay.Housing.PropertyValues;
using Eco.Gameplay.Items;
using Eco.Gameplay.Items.Recipes;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Occupancy;
using Eco.Gameplay.Property;
using Eco.Gameplay.Skills;
using Eco.Shared.Items;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Serialization;

namespace Eco.Mods.TechTree
{
    [Serialized]
    [RequireComponent(typeof(PropertyAuthComponent))]
    [RequireComponent(typeof(HousingComponent))]
    [RequireComponent(typeof(OccupancyRequirementComponent))]
    [RequireComponent(typeof(ForSaleComponent))]
    [RequireComponent(typeof(RoomRequirementsComponent))]
    [RequireRoomVolume(8)]
    [Tag("Usable")]
    [Ecopedia("Housing Objects", "Bedroom", subPageName: "Wardrobe4 Item")]
    public partial class Wardrobe4Object : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(Wardrobe4Item);
        public override LocString DisplayName => Localizer.DoStr("Wardrobe4");
        public override TableTextureMode TableTexture => (TableTextureMode)1;

        static Wardrobe4Object()
        {
            WorldObject.AddOccupancy<Wardrobe4Object>(new List<BlockOccupancy>
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
                new BlockOccupancy(new Vector3i(0, 0, 1)),
                new BlockOccupancy(new Vector3i(0, 1, 0)),
                new BlockOccupancy(new Vector3i(0, 1, 1)),
            });
        }

        protected override void Initialize()
        {
            this.GetComponent<HousingComponent>().HomeValue = Wardrobe4Item.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Wardrobe4")]
    [LocDescription("A simple wardrobe variant 4.")]
    [Ecopedia("Housing Objects", "Bedroom", createAsSubPage: true)]
    [Tag("Housing")]
    [Tag("CanBeOnRug")]
    [Weight(2000)]
    public partial class Wardrobe4Item : WorldObjectItem<Wardrobe4Object>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Wardrobe4"),
            Category = HousingConfig.GetRoomCategory("Bedroom"),
            BaseValue = 2f,
            TypeForRoomLimit = Localizer.DoStr("Wardrobe"),
            DiminishingReturnMultiplier = 0.9f,
        };

        protected override OccupancyContext GetOccupancyContext =>
            new SideAttachedContext((DirectionAxisFlags)32, WorldObject.GetOccupancyInfo(this.WorldObjectType));

        public override HomeFurnishingValue HomeValue => homeValue;
    }

    [RequiresSkill(typeof(CarpentrySkill), 2)]
    [Ecopedia("Housing Objects", "Bedroom", subPageName: "Wardrobe4 Item")]
    public partial class Wardrobe4Recipe : RecipeFamily
    {
        public Wardrobe4Recipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Wardrobe4",
                displayName: Localizer.DoStr("Wardrobe4"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("HewnLog", 15f, typeof(CarpentrySkill)),
                    new IngredientElement("WoodBoard", 10f, typeof(CarpentrySkill)),
                    new IngredientElement("Fabric", 3f, typeof(CarpentrySkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<Wardrobe4Item>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(CarpentrySkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(Wardrobe4Recipe), 2f, typeof(CarpentrySkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Wardrobe4"), typeof(Wardrobe4Recipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(CarpentryTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
