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
    [Ecopedia("Housing Objects", "Seating", subPageName: "Sofa 90 Degree Brown Item")]
    public partial class Sofa90DegreeBrownObject : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(Sofa90DegreeBrownItem);
        public override LocString DisplayName => Localizer.DoStr("Sofa 90 Degree Brown");
        public override TableTextureMode TableTexture => (TableTextureMode)1;

        static Sofa90DegreeBrownObject()
        {
            WorldObject.AddOccupancy<Sofa90DegreeBrownObject>(new List<BlockOccupancy>
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
                new BlockOccupancy(new Vector3i(1, 0, 0)),
                new BlockOccupancy(new Vector3i(2, 0, 0)),
                new BlockOccupancy(new Vector3i(0, 0, 1)),
                new BlockOccupancy(new Vector3i(0, 0, 2)),
                new BlockOccupancy(new Vector3i(1, 0, 1)),
                new BlockOccupancy(new Vector3i(1, 0, 2)),
                new BlockOccupancy(new Vector3i(2, 0, 2)),
                new BlockOccupancy(new Vector3i(2, 0, 1)),
            });
        }

        protected override void Initialize()
        {
            this.GetComponent<HousingComponent>().HomeValue = Sofa90DegreeBrownItem.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Sofa 90 Degree Brown")]
    [LocDescription("A simple sofa 90 degree brown color.")]
    [Ecopedia("Housing Objects", "Seating", createAsSubPage: true)]
    [Tag("Housing")]
    [Tag("CanBeOnRug")]
    [Weight(2000)]
    public partial class Sofa90DegreeBrownItem : WorldObjectItem<Sofa90DegreeBrownObject>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Sofa 90 Degree Brown"),
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
    [Ecopedia("Housing Objects", "Seating", subPageName: "Sofa 90 Degree Brown Item")]
    public partial class Sofa90DegreeBrownRecipe : RecipeFamily
    {
        public Sofa90DegreeBrownRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Sofa 90 Degree Brown",
                displayName: Localizer.DoStr("Sofa 90 Degree Brown"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("HewnLog", 20f, typeof(TailoringSkill)),
                    new IngredientElement("WoodBoard", 8f, typeof(TailoringSkill)),
                    new IngredientElement("Fabric", 6f, typeof(TailoringSkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<Sofa90DegreeBrownItem>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(TailoringSkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(Sofa90DegreeBrownRecipe), 2f, typeof(TailoringSkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Sofa 90 Degree Brown"), typeof(Sofa90DegreeBrownRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(TailoringTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
