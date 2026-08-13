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
    [Ecopedia("Housing Objects", "Seating", subPageName: "Sofa Brown No Legs Item")]
    public partial class SofaNoLegsBrownObject : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(SofaNoLegsBrownItem);
        public override LocString DisplayName => Localizer.DoStr("Sofa Brown No Legs");
        public override TableTextureMode TableTexture => (TableTextureMode)1;

        static SofaNoLegsBrownObject()
        {
            WorldObject.AddOccupancy<SofaNoLegsBrownObject>(new List<BlockOccupancy>
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
                new BlockOccupancy(new Vector3i(0, 0, 1)),
                new BlockOccupancy(new Vector3i(0, 0, 2)),
            });
        }

        protected override void Initialize()
        {
            this.GetComponent<HousingComponent>().HomeValue = SofaNoLegsBrownItem.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Sofa Brown No Legs")]
    [LocDescription("A simple sofa brown color without legs.")]
    [Ecopedia("Housing Objects", "Seating", createAsSubPage: true)]
    [Tag("Housing")]
    [Tag("CanBeOnRug")]
    [Weight(2000)]
    public partial class SofaNoLegsBrownItem : WorldObjectItem<SofaNoLegsBrownObject>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Sofa Brown No Legs"),
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
    [Ecopedia("Housing Objects", "Seating", subPageName: "Sofa Brown No Legs Item")]
    public partial class SofaNoLegsBrownRecipe : RecipeFamily
    {
        public SofaNoLegsBrownRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Sofa Brown No Legs",
                displayName: Localizer.DoStr("Sofa Brown No Legs"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("HewnLog", 15f, typeof(TailoringSkill)),
                    new IngredientElement("WoodBoard", 6f, typeof(TailoringSkill)),
                    new IngredientElement("Fabric", 5f, typeof(TailoringSkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<SofaNoLegsBrownItem>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(TailoringSkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(SofaNoLegsBrownRecipe), 2f, typeof(TailoringSkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Sofa Brown No Legs"), typeof(SofaNoLegsBrownRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(TailoringTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
