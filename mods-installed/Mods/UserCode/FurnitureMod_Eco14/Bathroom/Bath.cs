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
    [Ecopedia("Housing Objects", "Bathroom", subPageName: "Bath Item")]
    public partial class BathObject : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(BathItem);
        public override LocString DisplayName => Localizer.DoStr("Bath");
        public override TableTextureMode TableTexture => (TableTextureMode)6;

        static BathObject()
        {
            WorldObject.AddOccupancy<BathObject>(new List<BlockOccupancy>
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
                new BlockOccupancy(new Vector3i(0, 0, 1)),
            });
        }

        protected override void Initialize()
        {
            this.GetComponent<HousingComponent>().HomeValue = BathItem.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Bath")]
    [LocDescription("A simple bath to use on early stages of the game.")]
    [Ecopedia("Housing Objects", "Bathroom", createAsSubPage: true)]
    [Tag("Housing")]
    [Tag("CanBeOnRug")]
    [Weight(2000)]
    public partial class BathItem : WorldObjectItem<BathObject>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Bath"),
            Category = HousingConfig.GetRoomCategory("Bathroom"),
            BaseValue = 2f,
            TypeForRoomLimit = Localizer.DoStr("Bath"),
            DiminishingReturnMultiplier = 0.1f,
        };

        protected override OccupancyContext GetOccupancyContext =>
            new SideAttachedContext((DirectionAxisFlags)32, WorldObject.GetOccupancyInfo(this.WorldObjectType));

        public override HomeFurnishingValue HomeValue => homeValue;
    }

    [RequiresSkill(typeof(MasonrySkill), 2)]
    [Ecopedia("Housing Objects", "Bathroom", subPageName: "Bath Item")]
    public partial class BathRecipe : RecipeFamily
    {
        public BathRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Bath",
                displayName: Localizer.DoStr("Bath"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(MortaredLimestoneItem), 20f, typeof(MasonrySkill)),
                    new IngredientElement(typeof(SandItem), 10f, typeof(MasonrySkill)),
                    new IngredientElement(typeof(ClayItem), 10f, typeof(MasonrySkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<BathItem>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(MasonrySkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(BathRecipe), 2f, typeof(MasonrySkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Bath"), typeof(BathRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(MasonryTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
