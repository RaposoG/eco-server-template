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
    [Ecopedia("Housing Objects", "Bedroom", subPageName: "Bed1 Item")]
    public partial class Bed1Object : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(Bed1Item);
        public override LocString DisplayName => Localizer.DoStr("Bed1");
        public override TableTextureMode TableTexture => (TableTextureMode)1;

        static Bed1Object()
        {
            WorldObject.AddOccupancy<Bed1Object>(new List<BlockOccupancy>
            {
                new BlockOccupancy(new Vector3i(0, 0, 0)),
                new BlockOccupancy(new Vector3i(0, 0, 1)),
            });
        }

        protected override void Initialize()
        {
            this.GetComponent<HousingComponent>().HomeValue = Bed1Item.homeValue;
        }
    }

    [Serialized]
    [LocDisplayName("Bed1")]
    [LocDescription("A simple bed to sleep variant 1.")]
    [Ecopedia("Housing Objects", "Bedroom", createAsSubPage: true)]
    [Tag("Housing")]
    [Tag("CanBeOnRug")]
    [Weight(2000)]
    public partial class Bed1Item : WorldObjectItem<Bed1Object>
    {
        public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue
        {
            ObjectName = Localizer.DoStr("Bed1"),
            Category = HousingConfig.GetRoomCategory("Bedroom"),
            BaseValue = 5f,
            TypeForRoomLimit = Localizer.DoStr("Bed"),
            DiminishingReturnMultiplier = 0.6f,
        };

        protected override OccupancyContext GetOccupancyContext =>
            new SideAttachedContext((DirectionAxisFlags)32, WorldObject.GetOccupancyInfo(this.WorldObjectType));

        public override HomeFurnishingValue HomeValue => homeValue;
    }

    [RequiresSkill(typeof(LoggingSkill), 3)]
    [Ecopedia("Housing Objects", "Bedroom", subPageName: "Bed1 Item")]
    public partial class Bed1Recipe : RecipeFamily
    {
        public Bed1Recipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "Bed1",
                displayName: Localizer.DoStr("Bed1"),
                ingredients: new List<IngredientElement>
                {
                    new IngredientElement("HewnLog", 15f, typeof(LoggingSkill)),
                    new IngredientElement("WoodBoard", 10f, typeof(LoggingSkill)),
                    new IngredientElement("Fabric", 3f, typeof(LoggingSkill)),
                },
                items: new List<CraftingElement>
                {
                    new CraftingElement<Bed1Item>(1f),
                });

            this.Recipes = new List<Recipe> { recipe };
            this.ExperienceOnCraft = 2f;
            this.LaborInCalories = CreateLaborInCaloriesValue(60f, typeof(LoggingSkill));
            this.CraftMinutes = CreateCraftTimeValue(typeof(Bed1Recipe), 2f, typeof(LoggingSkill));

            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("Bed1"), typeof(Bed1Recipe));
            this.ModsPostInitialize();
            CraftingComponent.AddRecipe(typeof(CarpentryTableObject), this);
        }

        partial void ModsPreInitialize();
        partial void ModsPostInitialize();
    }
}
