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
    [Ecopedia("Housing Objects", "Chair", subPageName: "Chair1 Item")]
    public partial class Chair1Object : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(Chair1Item);
        public override LocString DisplayName => Localizer.DoStr("Chair1");
        public override TableTextureMode TableTexture => (TableTextureMode)1;

        static Chair1Object()
        {
            WorldObject.AddOccupancy<Chair1Object>(new List<BlockOccupancy>
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
            });
        }

        protected override void Initialize()
        {
            this.GetComponent<HousingComponent>().HomeValue = Chair1Item.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Chair1")]
    [LocDescription("A simple wooden chair variant 1.")]
    [Ecopedia("Housing Objects", "Seating", createAsSubPage: true)]
    [Tag("Housing")]
    [Tag("CanBeOnRug")]
    [Weight(2000)]
    public partial class Chair1Item : WorldObjectItem<Chair1Object>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Chair1"),
            Category = HousingConfig.GetRoomCategory("Seating"),
            BaseValue = 1.5f,
            TypeForRoomLimit = Localizer.DoStr("Chair"),
            DiminishingReturnMultiplier = 0.9f,
        };

        protected override OccupancyContext GetOccupancyContext =>
            new SideAttachedContext((DirectionAxisFlags)32, WorldObject.GetOccupancyInfo(this.WorldObjectType));

        public override HomeFurnishingValue HomeValue => homeValue;
    }

    [RequiresSkill(typeof(CarpentrySkill), 2)]
    [Ecopedia("Housing Objects", "Seating", subPageName: "Chair1 Item")]
    public partial class Chair1Recipe : RecipeFamily
    {
        public Chair1Recipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Chair1",
                displayName: Localizer.DoStr("Chair1"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("HewnLog", 10f, typeof(CarpentrySkill)),
                    new IngredientElement("WoodBoard", 4f, typeof(CarpentrySkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<Chair1Item>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(CarpentrySkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(Chair1Recipe), 2f, typeof(CarpentrySkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Chair1"), typeof(Chair1Recipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(CarpentryTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
