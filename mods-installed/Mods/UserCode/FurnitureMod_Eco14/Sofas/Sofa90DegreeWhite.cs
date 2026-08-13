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
    [Ecopedia("Housing Objects", "Seating", subPageName: "Sofa 90 Degree White Item")]
    public partial class Sofa90DegreeWhiteObject : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(Sofa90DegreeWhiteItem);
        public override LocString DisplayName => Localizer.DoStr("Sofa 90 Degree White");
        public override TableTextureMode TableTexture => (TableTextureMode)1;

        static Sofa90DegreeWhiteObject()
        {
            WorldObject.AddOccupancy<Sofa90DegreeWhiteObject>(new List<BlockOccupancy>
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
            this.GetComponent<HousingComponent>().HomeValue = Sofa90DegreeWhiteItem.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Sofa 90 Degree White")]
    [LocDescription("A simple sofa 90 degree white color.")]
    [Ecopedia("Housing Objects", "Seating", createAsSubPage: true)]
    [Tag("Housing")]
    [Tag("CanBeOnRug")]
    [Weight(2000)]
    public partial class Sofa90DegreeWhiteItem : WorldObjectItem<Sofa90DegreeWhiteObject>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Sofa 90 Degree White"),
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
    [Ecopedia("Housing Objects", "Seating", subPageName: "Sofa 90 Degree White Item")]
    public partial class Sofa90DegreeWhiteRecipe : RecipeFamily
    {
        public Sofa90DegreeWhiteRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Sofa 90 Degree White",
                displayName: Localizer.DoStr("Sofa 90 Degree White"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("HewnLog", 20f, typeof(TailoringSkill)),
                    new IngredientElement("WoodBoard", 8f, typeof(TailoringSkill)),
                    new IngredientElement("Fabric", 6f, typeof(TailoringSkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<Sofa90DegreeWhiteItem>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(TailoringSkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(Sofa90DegreeWhiteRecipe), 2f, typeof(TailoringSkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Sofa 90 Degree White"), typeof(Sofa90DegreeWhiteRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(TailoringTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
