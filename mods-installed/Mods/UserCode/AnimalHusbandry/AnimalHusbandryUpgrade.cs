
namespace Eco.Mods.TechTree
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Eco.Gameplay.Blocks;
    using Eco.Gameplay.Components;
    using Eco.Gameplay.DynamicValues;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Modules;
    using Eco.Gameplay.Objects;
    using Eco.Gameplay.Players;
    using Eco.Gameplay.Skills;
    using Eco.Gameplay.Systems;
    using Eco.Gameplay.Systems.TextLinks;
    using Eco.Shared.Localization;
    using Eco.Shared.Serialization;
    using Eco.Shared.Utils;
    using Eco.Core.Items;
    using Eco.World;
    using Eco.World.Blocks;
    using Eco.Gameplay.Pipes;
    using Eco.Core.Controller;
    using Eco.Gameplay.Items.Recipes;
    using Eco.Gameplay.Garbage;
    using Eco.Gameplay.Bonuses;

    [RequiresSkill(typeof(AnimalHusbandrySkill), 1)]      
    public partial class AnimalHusbandryUpgradeRecipe :
        RecipeFamily
    {
        public AnimalHusbandryUpgradeRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "AnimalHusbandryUpgrade",  //noloc
                displayName: Localizer.DoStr("AnimalHusbandry Upgrade"),

                // Defines the ingredients needed to craft this recipe. An ingredient items takes the following inputs
                // type of the item, the amount of the item, the skill required, and the talent used.
                ingredients: new List<IngredientElement>
                {
               new IngredientElement(typeof(CowsItem), 1, true),
               new IngredientElement(typeof(SheepItem), 1, true),
               new IngredientElement(typeof(RabbitItem), 1, true),      
                },
                garbages: new List<GarbageOutput>
                {
                    new GarbageOutput(typeof(Trash), 0.2f),
                },
                // Define our recipe output items.
                // For every output item there needs to be one CraftingElement entry with the type of the final item and the amount
                // to create.
                items: new List<CraftingElement>
                {
                        new CraftingElement<AnimalHusbandryUpgradeItem>(), 
                });
            this.Recipes = new List<Recipe> { recipe };


            this.ExperienceOnCraft = 4;  

            this.LaborInCalories = CreateLaborInCaloriesValue(5000, typeof(AnimalHusbandrySkill)); 
            this.CraftMinutes = CreateCraftTimeValue(typeof(AnimalHusbandryUpgradeRecipe), 10, typeof(AnimalHusbandrySkill), typeof(AnimalHusbandryFocusedSpeedTalent), typeof(AnimalHusbandryParallelSpeedTalent));     
            this.ModsPreInitialize();
            this.Initialize(Localizer.DoStr("AnimalHusbandry Upgrade"), typeof(AnimalHusbandryUpgradeRecipe));
            this.ModsPostInitialize();

            CraftingComponent.AddRecipe(typeof(AnimalFeederObject), this);
        }

        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();
        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();
    }

    [Serialized]
    [LocDisplayName("AnimalHusbandry Upgrade")]
    [LocDescription("Upgrade that greatly increases efficiency when crafting Animal Husbandry recipes.")]
    [Weight(1)] 
    [SalvageCost(typeof(Trash), 1.0f)]     
    [Ecopedia("Upgrade Modules", "Specialty Upgrades", createAsSubPage: true)]                                                                           
    [Tag("Upgrade")]
    [Tag("SpecialtyModule")]
    public partial class AnimalHusbandryUpgradeItem :
        EfficiencyModule 
    {

        public AnimalHusbandryUpgradeItem() : base(ModuleTypes.None, 1f) { }

        public override float MaterialTierBump => 0f;

        public override IEnumerable<Bonus> Bonuses => new[]
        {
            new Bonus
            {
                Causes  = new List<BonusCause>  { new CraftBonusCause { Action = BonusAction.ResourceCost,SkillTypes = new HashSet<Type> { typeof(AnimalHusbandrySkill) } } },
                Effects = new List<BonusEffect> { new BonusEffectAdditivePercent { Percent = -0.05f, LowerIsBetter = true  } },
            },
            new Bonus
            {
                Causes  = new List<BonusCause>  { new CraftBonusCause { Action = BonusAction.CraftTime,SkillTypes = new HashSet<Type> { typeof(AnimalHusbandrySkill) } } },
                Effects = new List<BonusEffect> { new BonusEffectMultiplicative { Value = 0.75f, LowerIsBetter = true  } },
            },
        };
    }
}

