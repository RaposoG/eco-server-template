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
    // Level 3 - Juice Master - Reduces craft cost of Mixology by 15% and increases juices production by 1
    // Level 3 - All the gains - Reduces craft cost of Mixology by 15% and additional 25% to protein smoothie
    // Level 6 - Field & Forest Master -  15% Reduction for Forest and Field Smoothie
    // Level 6 - Tropical & Desert Master - 15% Reduction for Desert and Tropical Smoothie
    #region Mixology

    public partial class MixologyFocusedWorkflowTalent : Talent
    {
        public MixologyFocusedWorkflowTalent()
        {
            this.Bonuses.Add(new Bonus
            {
                Name = Localizer.DoStr("Juice Master"),
                Causes = new List<BonusCause> { new CraftBonusCause { Action = BonusAction.Yield, SkillTypes = new HashSet<Type> { typeof(MixologySkill) },
                    Recipes = new HashSet<Type> {
                    typeof(AgaveJuiceRecipe),
                    typeof(TomatoJuiceRecipe),
                    typeof(PapayaJuiceRecipe),
                    typeof(PineappleJuiceRecipe),
                    typeof(HuckleberriesJuiceRecipe),
                    typeof(PricklyPearFruitJuiceRecipe),
                    } } },
                Effects = new List<BonusEffect> { new BonusEffectAdditive { Value = 1f } },
            });

            this.Bonuses.Add(new Bonus
            {
                Name = Localizer.DoStr("Juice Master"),
                Causes = new List<BonusCause> { new CraftBonusCause { Action = BonusAction.ResourceCost, SkillTypes = new HashSet<Type> { typeof(MixologySkill)} } },
                Effects = new List<BonusEffect> { new BonusEffectMultiplicative { Value = 0.85f, LowerIsBetter = true } },
            });
        }
    }

    public partial class MixologyLavishProteinTalent : Talent
    {
        public MixologyLavishProteinTalent()
        {
            this.Bonuses.Add(new Bonus
            {
                Name = Localizer.DoStr("All the gains"),
                Causes = new List<BonusCause> { new CraftBonusCause { Action = BonusAction.ResourceCost, Recipes = new HashSet<System.Type> { typeof(ProteinSmoothieRecipe) } } },
                Effects = new List<BonusEffect> { new BonusEffectMultiplicative { Value = 0.75f, LowerIsBetter = true } },
            });
            this.Bonuses.Add(new Bonus
            {
                Name = Localizer.DoStr("All the gains"),
                Causes = new List<BonusCause> { new CraftBonusCause { Action = BonusAction.ResourceCost, SkillTypes = new HashSet<Type> { typeof(MixologySkill) } } },
                Effects = new List<BonusEffect> { new BonusEffectMultiplicative { Value = 0.85f, LowerIsBetter = true } },
            });
        }
    }

    public partial class MixologyFieldExpertTalent : Talent
    {
        public MixologyFieldExpertTalent()
        {
            this.Bonuses.Add(new Bonus
            {
                Name = Localizer.DoStr("Field & Forest Master"),
                Causes = new List<BonusCause> { new CraftBonusCause { Action = BonusAction.ResourceCost, Recipes = new HashSet<System.Type> { typeof(FieldSmoothieRecipe) } } },
                Effects = new List<BonusEffect> { new BonusEffectMultiplicative { Value = 0.85f, LowerIsBetter = true } },
            });
            this.Bonuses.Add(new Bonus
            {
                Name = Localizer.DoStr("Field & Forest Master"),
                Causes = new List<BonusCause> { new CraftBonusCause { Action = BonusAction.ResourceCost, Recipes = new HashSet<System.Type> { typeof(ForestSmoothieRecipe) } } },
                Effects = new List<BonusEffect> { new BonusEffectMultiplicative { Value = 0.85f, LowerIsBetter = true } },
            });
        }
    }

    public partial class MixologyDesertExpertTalent : Talent
    {
        public MixologyDesertExpertTalent()
        {
            this.Bonuses.Add(new Bonus
            {
                Name = Localizer.DoStr("Tropical & Desert Master"),
                Causes = new List<BonusCause> { new CraftBonusCause { Action = BonusAction.ResourceCost, Recipes = new HashSet<System.Type> { typeof(TropicalSmoothieRecipe) } } },
                Effects = new List<BonusEffect> { new BonusEffectMultiplicative { Value = 0.85f, LowerIsBetter = true } },
            });
            this.Bonuses.Add(new Bonus
            {
                Name = Localizer.DoStr("Tropical & Desert Master"),
                Causes = new List<BonusCause> { new CraftBonusCause { Action = BonusAction.ResourceCost, Recipes = new HashSet<System.Type> { typeof(DesertSmoothieRecipe) } } },
                Effects = new List<BonusEffect> { new BonusEffectMultiplicative { Value = 0.85f, LowerIsBetter = true } },
            });
        }
    }

    #endregion

    #endregion
}
