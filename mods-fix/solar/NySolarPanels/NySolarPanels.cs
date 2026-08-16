
namespace Eco.Mods.TechTree
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using Eco.Core.Items;
	using Eco.Gameplay.Blocks;
	using Eco.Gameplay.Components;
	using Eco.Gameplay.Components.Auth;
	using Eco.Gameplay.DynamicValues;
	using Eco.Gameplay.Economy;
	using Eco.Gameplay.Housing;
	using Eco.Gameplay.Interactions;
	using Eco.Gameplay.Items;
	using Eco.Gameplay.Modules;
	using Eco.Gameplay.Minimap;
	using Eco.Gameplay.Objects;
	using Eco.Gameplay.Players;
	using Eco.Gameplay.Property;
	using Eco.Gameplay.Skills;
	using Eco.Gameplay.Systems;
	using Eco.Gameplay.Systems.TextLinks;
	using Eco.Gameplay.Pipes.LiquidComponents;
	using Eco.Gameplay.Pipes.Gases;
	using Eco.Shared;
	using Eco.Shared.Math;
	using Eco.Shared.Localization;
	using Eco.Shared.Serialization;
	using Eco.Shared.Utils;
	using Eco.Shared.View;
	using Eco.Shared.Items;
	using Eco.Shared.Networking;
	using Eco.Gameplay.Pipes;
	using Eco.World.Blocks;
	using Eco.Gameplay.Housing.PropertyValues;
	using Eco.Gameplay.Civics.Objects;
	using Eco.Gameplay.Settlements;
	using Eco.Gameplay.Systems.NewTooltip;
	using Eco.Core.Controller;
	using Eco.Core.Utils;
	using Eco.Gameplay.Components.Storage;
	using static Eco.Gameplay.Housing.PropertyValues.HomeFurnishingValue;
	using Eco.Gameplay.Items.Recipes;

	[Serialized]
	[RequireComponent(typeof(OnOffComponent))]
	[RequireComponent(typeof(PropertyAuthComponent))]
	[RequireComponent(typeof(PowerGridComponent))]
	[RequireComponent(typeof(PowerGeneratorComponent))]
	[RequireComponent(typeof(HousingComponent))]
	[RequireComponent(typeof(ForSaleComponent))]
	[PowerGenerator(typeof(ElectricPower))]

	public partial class NySolarPanelsObject : WorldObject, IRepresentsItem
	{
		public virtual Type RepresentedItemType => typeof(NySolarPanelsItem);
		public override LocString DisplayName => Localizer.DoStr("Solar Panels");
		public override TableTextureMode TableTexture => TableTextureMode.Metal;

		protected override void Initialize()
		{
			this.ModsPreInitialize();
			this.GetComponent<PowerGridComponent>().Initialize(30, new ElectricPower());
			this.GetComponent<PowerGeneratorComponent>().Initialize(200);
			this.GetComponent<HousingComponent>().HomeValue = NySolarPanelsItem.homeValue;
			this.ModsPostInitialize();
		}

		/// <summary>Hook for mods to customize WorldObject before initialization. You can change housing values here.</summary>
		partial void ModsPreInitialize();
		/// <summary>Hook for mods to customize WorldObject after initialization.</summary>
		partial void ModsPostInitialize();
	}

	[Serialized]
	[LocDisplayName("Solar Panels")]
	[LocDescription("Solar Panels for green energy production.")]
	[Ecopedia("Crafted Objects", "Power Generation", createAsSubPage: true)]
	[Weight(500)] // Defines how heavy WoodenCeilingLight is.
	public partial class NySolarPanelsItem : WorldObjectItem<NySolarPanelsObject>
	{
		public override HomeFurnishingValue HomeValue => homeValue;
		public static readonly HomeFurnishingValue homeValue = new HomeFurnishingValue()
		{
			Category = RoomCategory.Industrial,
			TypeForRoomLimit = Localizer.DoStr(""),
		};

		[NewTooltip(CacheAs.SubType, 7)] public static LocString PowerConsumptionTooltip() => Localizer.Do($"Produces: {Text.Info(200)}w of {new ElectricPower().Name} power.");
	}

	[RequiresSkill(typeof(ElectronicsSkill), 1)]
	public partial class NySolarPanelsRecipe : RecipeFamily
	{
		public NySolarPanelsRecipe()
		{
			var recipe = new Recipe();
			recipe.Init(
				name: "SolarPanels",  //noloc
				displayName: Localizer.DoStr("Solar Panels"),
				ingredients: new List<IngredientElement>
				{
					new IngredientElement(typeof(SteelBarItem), 15, typeof(ElectronicsSkill)),
					new IngredientElement(typeof(CopperWiringItem), 20, typeof(ElectronicsSkill)),
					new IngredientElement(typeof(BasicCircuitItem), 5, typeof(ElectronicsSkill)),
				},
				items: new List<CraftingElement>
				{
					new CraftingElement<NySolarPanelsItem>()
				});
			this.Recipes = new List<Recipe> { recipe };
			this.ExperienceOnCraft = 10;
			this.LaborInCalories = CreateLaborInCaloriesValue(2000, typeof(ElectronicsSkill));
			this.CraftMinutes = CreateCraftTimeValue(beneficiary: typeof(NySolarPanelsRecipe), start: 40, skillType: typeof(ElectronicsSkill));
			this.ModsPreInitialize();
			this.Initialize(displayText: Localizer.DoStr("Solar Panels"), recipeType: typeof(NySolarPanelsRecipe));
			this.ModsPostInitialize();
			CraftingComponent.AddRecipe(tableType: typeof(ElectronicsAssemblyObject), this);
		}

		/// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
		partial void ModsPreInitialize();
		/// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
		partial void ModsPostInitialize();
	}
}
