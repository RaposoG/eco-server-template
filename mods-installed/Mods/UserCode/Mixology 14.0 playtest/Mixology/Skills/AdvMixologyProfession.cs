// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Eco.Mods.TechTree
{
    using Eco.Gameplay.Bonuses;
    using Eco.Gameplay.Players;
    using Eco.Gameplay.Skills;
    using Eco.Shared.Localization;
    using Eco.Shared.Math;
    using Eco.Shared.Serialization;
    using Eco.Simulation.WorldLayers;
    using System;
    using System.Collections.Generic;


    #region Chef Profession

    // Mixology Talents
    // Level 3 - Coffee Master - Reduces craft cost of Advanced Mixology by 10% and increases Black Coffee production by 1
    // Level 3 - Tea Lover - Reduces craft cost of Advanced Mixology by 10% and increases English Breakfast Tea production by 1
    // Level 6 - Cocktail Hands -  Increase craft speed of all advanced mixology recipes by 100%
    // Level 6 - Slow Hands - Slows down craft speed of all advanced mixology recipes by 50% and reduces ingridients cost by 10%
    #region Advanced Mixology

    public partial class CoffeeMasterTalent : Talent
    {
        public CoffeeMasterTalent()
        {
            this.Bonuses.Add(new Bonus
            {
                Name = Localizer.DoStr("Coffee Master"),
                Causes = new List<BonusCause> { new CraftBonusCause { Action = BonusAction.Yield, SkillTypes = new HashSet<Type> { typeof(AdvancedMixologySkill) },
                    Recipes = new HashSet<Type> {
                    typeof(BlackCoffeeRecipe),
                    } } },
                Effects = new List<BonusEffect> { new BonusEffectAdditive { Value = 1f } },
            });

            this.Bonuses.Add(new Bonus
            {
                Name = Localizer.DoStr("Coffee Master"),
                Causes = new List<BonusCause> { new CraftBonusCause { Action = BonusAction.ResourceCost, SkillTypes = new HashSet<Type> { typeof(AdvancedMixologySkill) } } },
                Effects = new List<BonusEffect> { new BonusEffectMultiplicative { Value = 0.9f, LowerIsBetter = true } },
            });
        }
    }

    public partial class TeaLoverTalent : Talent
    {
        public TeaLoverTalent()
        {
            this.Bonuses.Add(new Bonus
            {
                Name = Localizer.DoStr("Tea Lover"),
                Causes = new List<BonusCause> { new CraftBonusCause { Action = BonusAction.Yield, SkillTypes = new HashSet<Type> { typeof(AdvancedMixologySkill) },
                    Recipes = new HashSet<Type> {
                    typeof(EnglishBreakfastTeaRecipe),
                    } } },
                Effects = new List<BonusEffect> { new BonusEffectAdditive { Value = 1f } },
            });
            this.Bonuses.Add(new Bonus
            {
                Name = Localizer.DoStr("Tea Lover"),
                Causes = new List<BonusCause> { new CraftBonusCause { Action = BonusAction.ResourceCost, SkillTypes = new HashSet<Type> { typeof(AdvancedMixologySkill) } } },
                Effects = new List<BonusEffect> { new BonusEffectMultiplicative { Value = 0.9f, LowerIsBetter = true } },
            });
        }
    }

    public partial class CocktailHandsTalent : Talent
    {
        public CocktailHandsTalent()
            {
                this.Bonuses.Add(new Bonus
                {
                    Name = Localizer.DoStr("Cocktail Hands"),
                    Causes = new List<BonusCause> { new CraftBonusCause { Action = BonusAction.CraftTime, SkillTypes = new HashSet<Type> { typeof(AdvancedMixologySkill) } } },
                    Effects = new List<BonusEffect> { new BonusEffectMultiplicative { Value = 0.5f, LowerIsBetter = true } },
                });
            }
    }

    public partial class SlowHandsTalent : Talent
    {
        public SlowHandsTalent()
        {
            this.Bonuses.Add(new Bonus
            {
                Name = Localizer.DoStr("Slow Hands"),
                Causes = new List<BonusCause> { new CraftBonusCause { Action = BonusAction.CraftTime, SkillTypes = new HashSet<Type> { typeof(AdvancedMixologySkill) } } },
                Effects = new List<BonusEffect> { new BonusEffectMultiplicative { Value = 1.5f, LowerIsBetter = true } },
            });
            this.Bonuses.Add(new Bonus
            {
                Name = Localizer.DoStr("Slow Hands"),
                Causes = new List<BonusCause> { new CraftBonusCause { Action = BonusAction.ResourceCost, SkillTypes = new HashSet<Type> { typeof(AdvancedMixologySkill) } } },
                Effects = new List<BonusEffect> { new BonusEffectMultiplicative { Value = 0.9f, LowerIsBetter = true } },
            });
        }
    }

    #endregion

    #endregion
}
