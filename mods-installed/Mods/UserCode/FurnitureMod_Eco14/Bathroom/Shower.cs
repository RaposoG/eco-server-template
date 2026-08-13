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
    [Ecopedia("Housing Objects", "Bathroom", subPageName: "Shower Item")]
    public partial class ShowerObject : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(ShowerItem);
        public override LocString DisplayName => Localizer.DoStr("Shower");
        public override TableTextureMode TableTexture => (TableTextureMode)1;

        static ShowerObject()
        {
            WorldObject.AddOccupancy<ShowerObject>(new List<BlockOccupancy>
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
                new BlockOccupancy(new Vector3i(0, 1, 0)),
            });
        }

        protected override void Initialize()
        {
            this.GetComponent<HousingComponent>().HomeValue = ShowerItem.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Shower")]
    [LocDescription("A basic shower to fast refresh yourself after hadr work.")]
    [Ecopedia("Housing Objects", "Bathroom", createAsSubPage: true)]
    [Tag("Housing")]
    [Tag("CanBeOnRug")]
    [Weight(2000)]
    public partial class ShowerItem : WorldObjectItem<ShowerObject>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Shower"),
            Category = HousingConfig.GetRoomCategory("Bathroom"),
            BaseValue = 2f,
            TypeForRoomLimit = Localizer.DoStr("Shower"),
            DiminishingReturnMultiplier = 0.1f,
        };

        protected override OccupancyContext GetOccupancyContext =>
            new SideAttachedContext((DirectionAxisFlags)32, WorldObject.GetOccupancyInfo(this.WorldObjectType));

        public override HomeFurnishingValue HomeValue => homeValue;
    }

    [RequiresSkill(typeof(MasonrySkill), 2)]
    [Ecopedia("Housing Objects", "Bathroom", subPageName: "Shower Item")]
    public partial class ShowerRecipe : RecipeFamily
    {
        public ShowerRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Shower",
                displayName: Localizer.DoStr("Shower"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(MortaredLimestoneItem), 15f, typeof(MasonrySkill)),
                    new IngredientElement(typeof(SandItem), 6f, typeof(MasonrySkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<ShowerItem>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(MasonrySkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(ShowerRecipe), 2f, typeof(MasonrySkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Shower"), typeof(ShowerRecipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(MasonryTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
