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
    [Ecopedia("Housing Objects", "Living Room", subPageName: "Closet Item")]
    public partial class ClosetObject : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(ClosetItem);
        public override LocString DisplayName => Localizer.DoStr("Closet");
        public override TableTextureMode TableTexture => (TableTextureMode)1;

        static ClosetObject()
        {
            WorldObject.AddOccupancy<ClosetObject>(new List<BlockOccupancy>
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
                new BlockOccupancy(new Vector3i(0, 0, 1)),
                new BlockOccupancy(new Vector3i(0, 0, 2)),
                new BlockOccupancy(new Vector3i(0, 1, 0)),
                new BlockOccupancy(new Vector3i(0, 1, 1)),
                new BlockOccupancy(new Vector3i(0, 1, 2)),
                new BlockOccupancy(new Vector3i(0, 2, 0)),
                new BlockOccupancy(new Vector3i(0, 2, 1)),
                new BlockOccupancy(new Vector3i(0, 2, 2)),
            });
        }

        protected override void Initialize()
        {
            this.GetComponent<HousingComponent>().HomeValue = ClosetItem.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Closet")]
    [LocDescription("A simple PC table variant 1.")]
    [Ecopedia("Housing Objects", "Living Room", createAsSubPage: true)]
    [Tag("Housing")]
    [Tag("CanBeOnRug")]
    [Weight(2000)]
    public partial class ClosetItem : WorldObjectItem<ClosetObject>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Closet"),
            Category = HousingConfig.GetRoomCategory("Living Room"),
            BaseValue = 2f,
            TypeForRoomLimit = Localizer.DoStr("PC Table"),
            DiminishingReturnMultiplier = 0.9f,
        };

        protected override OccupancyContext GetOccupancyContext =>
            new SideAttachedContext((DirectionAxisFlags)32, WorldObject.GetOccupancyInfo(this.WorldObjectType));

        public override HomeFurnishingValue HomeValue => homeValue;
    }

    [RequiresSkill(typeof(CarpentrySkill), 2)]
    [Ecopedia("Housing Objects", "Living Room", subPageName: "Closet Item")]
    public partial class ClosetRecipe : RecipeFamily
    {
        public ClosetRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Closet",
                displayName: Localizer.DoStr("Closet"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("HewnLog", 15f, typeof(CarpentrySkill)),
                    new IngredientElement("WoodBoard", 10f, typeof(CarpentrySkill)),
                    new IngredientElement("Fabric", 3f, typeof(CarpentrySkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<ClosetItem>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(CarpentrySkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(ClosetRecipe), 2f, typeof(CarpentrySkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Closet"), typeof(ClosetRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(CarpentryTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
