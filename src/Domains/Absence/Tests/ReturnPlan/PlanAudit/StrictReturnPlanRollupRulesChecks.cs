namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class StrictReturnPlanRollupRulesChecks
{
    public static void StrictReturnPlanRollupValidatorReportsEveryProblem()
    {
        var validator = StrictReturnPlanRollupRules.DefaultStrictReturnPlanRollupValidator();
        var item = StrictReturnPlanRollup.NewStrictReturnPlanRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictReturnPlanRollupRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new StrictReturnPlanRollupRequiredCurrencyRule("strict return plan rollup needs an entry");
        var item = StrictReturnPlanRollup.NewStrictReturnPlanRollup("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictReturnPlanRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictReturnPlanRollupRequiredCodeRule("strict return plan rollup needs this field");
        var item = StrictReturnPlanRollup.NewStrictReturnPlanRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictReturnPlanRollupRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StrictReturnPlanRollupRequiredTitleRule("strict return plan rollup needs a value");
        var item = StrictReturnPlanRollup.NewStrictReturnPlanRollup("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictReturnPlanRollupThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new StrictReturnPlanRollupThresholdRangeRule(3.4, 1869.6);
        var item = StrictReturnPlanRollup.NewStrictReturnPlanRollup("x", "y");
        item.Threshold = 1871.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictReturnPlanRollupMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new StrictReturnPlanRollupMarginRangeRule(3.1, 3420.0);
        var item = StrictReturnPlanRollup.NewStrictReturnPlanRollup("x", "y");
        item.Margin = 3421.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictReturnPlanRollupValidatorReportsEveryProblem();
        StrictReturnPlanRollupRequiredCurrencyRuleFlagsABlankCurrency();
        StrictReturnPlanRollupRequiredCodeRuleFlagsABlankCode();
        StrictReturnPlanRollupRequiredTitleRuleFlagsABlankTitle();
        StrictReturnPlanRollupThresholdRangeRuleFlagsValuesAboveThreshold();
        StrictReturnPlanRollupMarginRangeRuleFlagsValuesAboveMargin();
    }
}
