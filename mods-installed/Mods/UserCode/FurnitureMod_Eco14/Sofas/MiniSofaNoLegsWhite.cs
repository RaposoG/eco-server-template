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
    [Ecopedia("Housing Objects", "Seating", subPageName: "Mini Sofa White No Legs Item")]
    public partial class MiniSofaNoLegsWhiteObject : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(MiniSofaNoLegsWhiteItem);
        public override LocString DisplayName => Localizer.DoStr("Mini Sofa White No Legs");
        public override TableTextureMode TableTexture => (TableTextureMode)1;

        static MiniSofaNoLegsWhiteObject()
        {
            WorldObject.AddOccupancy<MiniSofaNoLegsWhiteObject>(new List<BlockOccupancy>
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
            });
        }

        protected override void Initialize()
        {
            this.GetComponent<HousingComponent>().HomeValue = MiniSofaNoLegsWhiteItem.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Mini Sofa White No Legs")]
    [LocDescription("A simple mini sofa white color without legs.")]
    [Ecopedia("Housing Objects", "Seating", createAsSubPage: true)]
    [Tag("Housing")]
    [Tag("CanBeOnRug")]
    [Weight(2000)]
    public partial class MiniSofaNoLegsWhiteItem : WorldObjectItem<MiniSofaNoLegsWhiteObject>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Mini Sofa White No Legs"),
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
    [Ecopedia("Housing Objects", "Seating", subPageName: "Mini Sofa White No Legs Item")]
    public partial class MiniSofaNoLegsWhiteRecipe : RecipeFamily
    {
        public MiniSofaNoLegsWhiteRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Mini Sofa White No Legs",
                displayName: Localizer.DoStr("Mini Sofa White No Legs"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("HewnLog", 10f, typeof(TailoringSkill)),
                    new IngredientElement("WoodBoard", 4f, typeof(TailoringSkill)),
                    new IngredientElement("Fabric", 3f, typeof(TailoringSkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<MiniSofaNoLegsWhiteItem>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(TailoringSkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(MiniSofaNoLegsWhiteRecipe), 2f, typeof(TailoringSkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Mini Sofa White No Legs"), typeof(MiniSofaNoLegsWhiteRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(TailoringTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
