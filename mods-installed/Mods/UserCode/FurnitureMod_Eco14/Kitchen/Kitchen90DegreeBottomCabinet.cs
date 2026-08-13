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
    [Ecopedia("Housing Objects", "Kitchen", subPageName: "Kitchen 90 Degree Bottom Cabinet Item")]
    public partial class Kitchen90DegreeBottomCabinetObject : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(Kitchen90DegreeBottomCabinetItem);
        public override LocString DisplayName => Localizer.DoStr("Kitchen 90 Degree Bottom Cabinet");
        public override TableTextureMode TableTexture => (TableTextureMode)1;

        static Kitchen90DegreeBottomCabinetObject()
        {
            WorldObject.AddOccupancy<Kitchen90DegreeBottomCabinetObject>(new List<BlockOccupancy>
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
                new BlockOccupancy(new Vector3i(1, 0, 0)),
                new BlockOccupancy(new Vector3i(0, 0, 1)),
                new BlockOccupancy(new Vector3i(1, 0, 1)),
            });
        }

        protected override void Initialize()
        {
            this.GetComponent<HousingComponent>().HomeValue = Kitchen90DegreeBottomCabinetItem.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Kitchen 90 Degree Bottom Cabinet")]
    [LocDescription("A basic kitchen 90 degree bottom cabinet.")]
    [Ecopedia("Housing Objects", "Kitchen", createAsSubPage: true)]
    [Tag("Housing")]
    [Weight(2000)]
    public partial class Kitchen90DegreeBottomCabinetItem : WorldObjectItem<Kitchen90DegreeBottomCabinetObject>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Kitchen 90 Degree Bottom Cabinet"),
            Category = HousingConfig.GetRoomCategory("Kitchen"),
            BaseValue = 2f,
            TypeForRoomLimit = Localizer.DoStr("Bottom Cabinet"),
            DiminishingReturnMultiplier = 0.9f,
        };

        protected override OccupancyContext GetOccupancyContext =>
            new SideAttachedContext((DirectionAxisFlags)32, WorldObject.GetOccupancyInfo(this.WorldObjectType));

        public override HomeFurnishingValue HomeValue => homeValue;
    }

    [RequiresSkill(typeof(CarpentrySkill), 2)]
    [Ecopedia("Housing Objects", "Kitchen", subPageName: "Kitchen 90 Degree Bottom Cabinet Item")]
    public partial class Kitchen90DegreeBottomCabinetRecipe : RecipeFamily
    {
        public Kitchen90DegreeBottomCabinetRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Kitchen 90 Degree Bottom Cabinet",
                displayName: Localizer.DoStr("Kitchen 90 Degree Bottom Cabinet"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("HewnLog", 15f, typeof(CarpentrySkill)),
                    new IngredientElement("WoodBoard", 25f, typeof(CarpentrySkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<Kitchen90DegreeBottomCabinetItem>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(CarpentrySkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(Kitchen90DegreeBottomCabinetRecipe), 2f, typeof(CarpentrySkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Kitchen 90 Degree Bottom Cabinet"), typeof(Kitchen90DegreeBottomCabinetRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(CarpentryTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
