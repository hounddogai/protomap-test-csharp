namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Score;

using Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictSuccessionPlanTierRulesChecks
{
    public static void StrictSuccessionPlanTierValidatorReportsEveryProblem()
    {
        var validator = StrictSuccessionPlanTierRules.DefaultStrictSuccessionPlanTierValidator();
        var item = StrictSuccessionPlanTier.NewStrictSuccessionPlanTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictSuccessionPlanTierFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new StrictSuccessionPlanTierFteRangeRule(2.6, 67.2);
        var item = StrictSuccessionPlanTier.NewStrictSuccessionPlanTier("x", "y");
        item.Fte = 68.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictSuccessionPlanTierCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new StrictSuccessionPlanTierCeilingRangeRule(0.5, 4825.7);
        var item = StrictSuccessionPlanTier.NewStrictSuccessionPlanTier("x", "y");
        item.Ceiling = 4827.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictSuccessionPlanTierRequiredNameRuleFlagsABlankName()
    {
        var rule = new StrictSuccessionPlanTierRequiredNameRule("strict succession plan tier needs this field");
        var item = StrictSuccessionPlanTier.NewStrictSuccessionPlanTier("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictSuccessionPlanTierCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new StrictSuccessionPlanTierCapacityRangeRule(80, 120);
        var item = StrictSuccessionPlanTier.NewStrictSuccessionPlanTier("x", "y");
        item.Capacity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictSuccessionPlanTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictSuccessionPlanTierCodeLengthRule(1000);
        var item = StrictSuccessionPlanTier.NewStrictSuccessionPlanTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictSuccessionPlanTierBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new StrictSuccessionPlanTierBudgetRangeRule(5.3, 1776.5);
        var item = StrictSuccessionPlanTier.NewStrictSuccessionPlanTier("x", "y");
        item.Budget = 1778.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictSuccessionPlanTierValidatorReportsEveryProblem();
        StrictSuccessionPlanTierFteRangeRuleFlagsValuesAboveFte();
        StrictSuccessionPlanTierCeilingRangeRuleFlagsValuesAboveCeiling();
        StrictSuccessionPlanTierRequiredNameRuleFlagsABlankName();
        StrictSuccessionPlanTierCapacityRangeRuleFlagsValuesAboveCapacity();
        StrictSuccessionPlanTierCodeLengthRuleFlagsALongCode();
        StrictSuccessionPlanTierBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
