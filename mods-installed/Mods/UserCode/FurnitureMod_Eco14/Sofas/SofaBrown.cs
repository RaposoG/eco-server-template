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
    [Ecopedia("Housing Objects", "Seating", subPageName: "Sofa Brown Item")]
    public partial class SofaBrownObject : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(SofaBrownItem);
        public override LocString DisplayName => Localizer.DoStr("Sofa Brown");
        public override TableTextureMode TableTexture => (TableTextureMode)1;

        static SofaBrownObject()
        {
            WorldObject.AddOccupancy<SofaBrownObject>(new List<BlockOccupancy>
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
                new BlockOccupancy(new Vector3i(0, 0, 1)),
                new BlockOccupancy(new Vector3i(0, 0, 2)),
            });
        }

        protected override void Initialize()
        {
            this.GetComponent<HousingComponent>().HomeValue = SofaBrownItem.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Sofa Brown")]
    [LocDescription("A simple sofa brown color.")]
    [Ecopedia("Housing Objects", "Seating", createAsSubPage: true)]
    [Tag("Housing")]
    [Tag("CanBeOnRug")]
    [Weight(2000)]
    public partial class SofaBrownItem : WorldObjectItem<SofaBrownObject>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Sofa Brown"),
            Category = HousingConfig.GetRoomCategory("Seating"),
            BaseValue = 2f,
            TypeForRoomLimit = Localizer.DoStr("Sofa"),
            DiminishingReturnMultiplier = 0.9f,
        };

        protected override OccupancyContext GetOccupancyContext =>
            new SideAttachedContext((DirectionAxisFlags)32, WorldObject.GetOccupancyInfo(this.WorldObjectType));

        public override HomeFurnishingValue HomeValue => homeValue;
    }

    [RequiresSkill(typeof(TailoringSkill), 2)]
    [Ecopedia("Housing Objects", "Seating", subPageName: "Sofa Brown Item")]
    public partial class SofaBrownRecipe : RecipeFamily
    {
        public SofaBrownRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Sofa Brown",
                displayName: Localizer.DoStr("Sofa Brown"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("HewnLog", 15f, typeof(TailoringSkill)),
                    new IngredientElement("WoodBoard", 6f, typeof(TailoringSkill)),
                    new IngredientElement("Fabric", 5f, typeof(TailoringSkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<SofaBrownItem>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(TailoringSkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(SofaBrownRecipe), 2f, typeof(TailoringSkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Sofa Brown"), typeof(SofaBrownRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(TailoringTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
