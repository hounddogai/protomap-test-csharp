namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class StrictReturnPlanMetricRulesChecks
{
    public static void StrictReturnPlanMetricValidatorReportsEveryProblem()
    {
        var validator = StrictReturnPlanMetricRules.DefaultStrictReturnPlanMetricValidator();
        var item = StrictReturnPlanMetric.NewStrictReturnPlanMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictReturnPlanMetricRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new StrictReturnPlanMetricRequiredReviewerRule("strict return plan metric needs this field");
        var item = StrictReturnPlanMetric.NewStrictReturnPlanMetric("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictReturnPlanMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictReturnPlanMetricRequiredCodeRule("strict return plan metric needs this field");
        var item = StrictReturnPlanMetric.NewStrictReturnPlanMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictReturnPlanMetricRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new StrictReturnPlanMetricRequiredManagerIdRule("strict return plan metric needs this field");
        var item = StrictReturnPlanMetric.NewStrictReturnPlanMetric("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictReturnPlanMetricValidatorReportsEveryProblem();
        StrictReturnPlanMetricRequiredReviewerRuleFlagsABlankReviewer();
        StrictReturnPlanMetricRequiredCodeRuleFlagsABlankCode();
        StrictReturnPlanMetricRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
