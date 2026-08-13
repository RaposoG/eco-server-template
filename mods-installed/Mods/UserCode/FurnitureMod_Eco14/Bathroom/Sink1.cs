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
    [Ecopedia("Housing Objects", "Bathroom", subPageName: "Sink1 Item")]
    public partial class Sink1Object : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(Sink1Item);
        public override LocString DisplayName => Localizer.DoStr("Sink1");
        public override TableTextureMode TableTexture => (TableTextureMode)1;

        static Sink1Object()
        {
            WorldObject.AddOccupancy<Sink1Object>(new List<BlockOccupancy>
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
            });
        }

        protected override void Initialize()
        {
            this.GetComponent<HousingComponent>().HomeValue = Sink1Item.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Sink1")]
    [LocDescription("A simple sink to wash your face and shave yourself.")]
    [Ecopedia("Housing Objects", "Bathroom", createAsSubPage: true)]
    [Tag("Housing")]
    [Tag("CanBeOnRug")]
    [Weight(2000)]
    public partial class Sink1Item : WorldObjectItem<Sink1Object>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Sink1"),
            Category = HousingConfig.GetRoomCategory("Bathroom"),
            BaseValue = 2f,
            TypeForRoomLimit = Localizer.DoStr("Sink"),
            DiminishingReturnMultiplier = 0.1f,
        };

        protected override OccupancyContext GetOccupancyContext =>
            new SideAttachedContext((DirectionAxisFlags)32, WorldObject.GetOccupancyInfo(this.WorldObjectType));

        public override HomeFurnishingValue HomeValue => homeValue;
    }

    [RequiresSkill(typeof(MasonrySkill), 2)]
    [Ecopedia("Housing Objects", "Bathroom", subPageName: "Sink1 Item")]
    public partial class Sink1Recipe : RecipeFamily
    {
        public Sink1Recipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Sink1",
                displayName: Localizer.DoStr("Sink1"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(MortaredLimestoneItem), 15f, typeof(MasonrySkill)),
                    new IngredientElement(typeof(SandItem), 6f, typeof(MasonrySkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<Sink1Item>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(MasonrySkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(Sink1Recipe), 2f, typeof(MasonrySkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Sink1"), typeof(Sink1Recipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(MasonryTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
