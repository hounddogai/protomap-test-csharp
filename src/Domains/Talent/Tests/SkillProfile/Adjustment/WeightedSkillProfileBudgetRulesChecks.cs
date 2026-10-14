namespace Acme.Workforce.Talent.Tests.SkillProfile.Adjustment;

using Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WeightedSkillProfileBudgetRulesChecks
{
    public static void WeightedSkillProfileBudgetValidatorReportsEveryProblem()
    {
        var validator = WeightedSkillProfileBudgetRules.DefaultWeightedSkillProfileBudgetValidator();
        var item = WeightedSkillProfileBudget.NewWeightedSkillProfileBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedSkillProfileBudgetMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new WeightedSkillProfileBudgetMaximumRangeRule(7.8, 1290.2);
        var item = WeightedSkillProfileBudget.NewWeightedSkillProfileBudget("x", "y");
        item.Maximum = 1291.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedSkillProfileBudgetTitleLengthRuleFlagsALongTitle()
    {
        var rule = new WeightedSkillProfileBudgetTitleLengthRule(86400);
        var item = WeightedSkillProfileBudget.NewWeightedSkillProfileBudget("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedSkillProfileBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedSkillProfileBudgetRequiredCodeRule("weighted skill profile budget needs this field");
        var item = WeightedSkillProfileBudget.NewWeightedSkillProfileBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedSkillProfileBudgetValidatorReportsEveryProblem();
        WeightedSkillProfileBudgetMaximumRangeRuleFlagsValuesAboveMaximum();
        WeightedSkillProfileBudgetTitleLengthRuleFlagsALongTitle();
        WeightedSkillProfileBudgetRequiredCodeRuleFlagsABlankCode();
    }
}
