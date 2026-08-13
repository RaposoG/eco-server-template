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
    [Ecopedia("Housing Objects", "Kitchen", subPageName: "Kitchen Cabinet Stove Oven Item")]
    public partial class KitchenCabinetStoveOvenObject : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(KitchenCabinetStoveOvenItem);
        public override LocString DisplayName => Localizer.DoStr("Kitchen Cabinet Stove Oven");
        public override TableTextureMode TableTexture => (TableTextureMode)1;

        static KitchenCabinetStoveOvenObject()
        {
            WorldObject.AddOccupancy<KitchenCabinetStoveOvenObject>(new List<BlockOccupancy>
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
            });
        }

        protected override void Initialize()
        {
            this.GetComponent<HousingComponent>().HomeValue = KitchenCabinetStoveOvenItem.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Kitchen Cabinet Stove Oven")]
    [LocDescription("A simple kitchen cabinet with stove and oven.")]
    [Ecopedia("Housing Objects", "Kitchen", createAsSubPage: true)]
    [Tag("Housing")]
    [Tag("CanBeOnRug")]
    [Weight(2000)]
    public partial class KitchenCabinetStoveOvenItem : WorldObjectItem<KitchenCabinetStoveOvenObject>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Kitchen Cabinet Stove Oven"),
            Category = HousingConfig.GetRoomCategory("Kitchen"),
            BaseValue = 2f,
            TypeForRoomLimit = Localizer.DoStr("Stove"),
            DiminishingReturnMultiplier = 0.9f,
        };

        protected override OccupancyContext GetOccupancyContext =>
            new SideAttachedContext((DirectionAxisFlags)32, WorldObject.GetOccupancyInfo(this.WorldObjectType));

        public override HomeFurnishingValue HomeValue => homeValue;
    }

    [RequiresSkill(typeof(CarpentrySkill), 2)]
    [Ecopedia("Housing Objects", "Kitchen", subPageName: "Kitchen Cabinet Stove Oven Item")]
    public partial class KitchenCabinetStoveOvenRecipe : RecipeFamily
    {
        public KitchenCabinetStoveOvenRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Kitchen Cabinet Stove Oven",
                displayName: Localizer.DoStr("Kitchen Cabinet Stove Oven"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("HewnLog", 15f, typeof(CarpentrySkill)),
                    new IngredientElement("WoodBoard", 25f, typeof(CarpentrySkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<KitchenCabinetStoveOvenItem>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(CarpentrySkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(KitchenCabinetStoveOvenRecipe), 2f, typeof(CarpentrySkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Kitchen Cabinet Stove Oven"), typeof(KitchenCabinetStoveOvenRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(CarpentryTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
