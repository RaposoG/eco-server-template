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
    [Ecopedia("Housing Objects", "Kitchen", subPageName: "Kitchen Upper Cabinet Item")]
    public partial class KitchenUpperCabinetObject : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(KitchenUpperCabinetItem);
        public override LocString DisplayName => Localizer.DoStr("Kitchen Upper Cabinet");
        public override TableTextureMode TableTexture => (TableTextureMode)1;

        static KitchenUpperCabinetObject()
        {
            WorldObject.AddOccupancy<KitchenUpperCabinetObject>(new List<BlockOccupancy>
            {
                new BlockOccupancy(new Vector3i(0, 2, 0)),
            });
        }

        protected override void Initialize()
        {
            this.GetComponent<HousingComponent>().HomeValue = KitchenUpperCabinetItem.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Kitchen Upper Cabinet")]
    [LocDescription("A basic mirror to see yourself in the morning.")]
    [Ecopedia("Housing Objects", "Kitchen", createAsSubPage: true)]
    [Tag("Housing")]
    [Weight(2000)]
    public partial class KitchenUpperCabinetItem : WorldObjectItem<KitchenUpperCabinetObject>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Kitchen Upper Cabinet"),
            Category = HousingConfig.GetRoomCategory("Kitchen"),
            BaseValue = 2f,
            TypeForRoomLimit = Localizer.DoStr("Upper Cabinet"),
            DiminishingReturnMultiplier = 0.9f,
        };

        protected override OccupancyContext GetOccupancyContext =>
            new SideAttachedContext((DirectionAxisFlags)4, WorldObject.GetOccupancyInfo(this.WorldObjectType));

        public override HomeFurnishingValue HomeValue => homeValue;
    }

    [RequiresSkill(typeof(CarpentrySkill), 2)]
    [Ecopedia("Housing Objects", "Kitchen", subPageName: "Kitchen Upper Cabinet Item")]
    public partial class KitchenUpperCabinetRecipe : RecipeFamily
    {
        public KitchenUpperCabinetRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "BKitchen Upper Cabinet",
                displayName: Localizer.DoStr("Kitchen Upper Cabinet"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("HewnLog", 15f, typeof(CarpentrySkill)),
                    new IngredientElement("WoodBoard", 25f, typeof(CarpentrySkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<KitchenUpperCabinetItem>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(CarpentrySkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(KitchenUpperCabinetRecipe), 2f, typeof(CarpentrySkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Kitchen Upper Cabinet"), typeof(KitchenUpperCabinetRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(CarpentryTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
