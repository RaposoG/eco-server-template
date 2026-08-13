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
    [Ecopedia("Housing Objects", "Kitchen", subPageName: "Kitchen 90 Degree Upper Cabinet Item")]
    public partial class Kitchen90DegreeUpperCabinetObject : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(Kitchen90DegreeUpperCabinetItem);
        public override LocString DisplayName => Localizer.DoStr("Kitchen 90 Degree Upper Cabinet");
        public override TableTextureMode TableTexture => (TableTextureMode)1;

        static Kitchen90DegreeUpperCabinetObject()
        {
            WorldObject.AddOccupancy<Kitchen90DegreeUpperCabinetObject>(new List<BlockOccupancy>
            {
                new BlockOccupancy(new Vector3i(0, 2, 0)),
                new BlockOccupancy(new Vector3i(1, 2, 0)),
                new BlockOccupancy(new Vector3i(0, 2, 1)),
                new BlockOccupancy(new Vector3i(1, 2, 1)),
            });
        }

        protected override void Initialize()
        {
            this.GetComponent<HousingComponent>().HomeValue = Kitchen90DegreeUpperCabinetItem.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Kitchen 90 Degree Upper Cabinet")]
    [LocDescription("A basic mirror to see yourself in the morning.")]
    [Ecopedia("Housing Objects", "Kitchen", createAsSubPage: true)]
    [Tag("Housing")]
    [Weight(2000)]
    public partial class Kitchen90DegreeUpperCabinetItem : WorldObjectItem<Kitchen90DegreeUpperCabinetObject>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Kitchen 90 Degree Upper Cabinet"),
            Category = HousingConfig.GetRoomCategory("Kitchen"),
            BaseValue = 2f,
            TypeForRoomLimit = Localizer.DoStr("Upper Cabinet"),
            DiminishingReturnMultiplier = 0.9f,
        };

        protected override OccupancyContext GetOccupancyContext =>
            new SideAttachedContext((DirectionAxisFlags)8, WorldObject.GetOccupancyInfo(this.WorldObjectType));

        public override HomeFurnishingValue HomeValue => homeValue;
    }

    [RequiresSkill(typeof(CarpentrySkill), 2)]
    [Ecopedia("Housing Objects", "Kitchen", subPageName: "Kitchen 90 Degree Upper Cabinet Item")]
    public partial class Kitchen90DegreeUpperCabinetRecipe : RecipeFamily
    {
        public Kitchen90DegreeUpperCabinetRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "BKitchen 90 Degree Upper Cabinet",
                displayName: Localizer.DoStr("Kitchen 90 Degree Upper Cabinet"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("HewnLog", 15f, typeof(CarpentrySkill)),
                    new IngredientElement("WoodBoard", 25f, typeof(CarpentrySkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<Kitchen90DegreeUpperCabinetItem>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(CarpentrySkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(Kitchen90DegreeUpperCabinetRecipe), 2f, typeof(CarpentrySkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Kitchen 90 Degree Upper Cabinet"), typeof(Kitchen90DegreeUpperCabinetRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(CarpentryTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
