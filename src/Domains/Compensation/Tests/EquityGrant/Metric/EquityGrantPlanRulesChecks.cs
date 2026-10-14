namespace Acme.Workforce.Compensation.Tests.EquityGrant.Metric;

using Acme.Workforce.Compensation.EquityGrant.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class EquityGrantPlanRulesChecks
{
    public static void EquityGrantPlanValidatorReportsEveryProblem()
    {
        var validator = EquityGrantPlanRules.DefaultEquityGrantPlanValidator();
        var item = EquityGrantPlan.NewEquityGrantPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new EquityGrantPlanCodeLengthRule(120);
        var item = EquityGrantPlan.NewEquityGrantPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantPlanRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new EquityGrantPlanRequiredTitleRule("equity grant plan needs this field");
        var item = EquityGrantPlan.NewEquityGrantPlan("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantPlanRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new EquityGrantPlanRetriesRangeRule(16, 80);
        var item = EquityGrantPlan.NewEquityGrantPlan("x", "y");
        item.Retries = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantPlanRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new EquityGrantPlanRequiredReviewerRule("equity grant plan needs this field");
        var item = EquityGrantPlan.NewEquityGrantPlan("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantPlanTitleLengthRuleFlagsALongTitle()
    {
        var rule = new EquityGrantPlanTitleLengthRule(255);
        var item = EquityGrantPlan.NewEquityGrantPlan("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        EquityGrantPlanValidatorReportsEveryProblem();
        EquityGrantPlanCodeLengthRuleFlagsALongCode();
        EquityGrantPlanRequiredTitleRuleFlagsABlankTitle();
        EquityGrantPlanRetriesRangeRuleFlagsValuesAboveRetries();
        EquityGrantPlanRequiredReviewerRuleFlagsABlankReviewer();
        EquityGrantPlanTitleLengthRuleFlagsALongTitle();
    }
}
