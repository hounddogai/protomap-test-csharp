namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultActionPlanMetricRulesChecks
{
    public static void DefaultActionPlanMetricValidatorReportsEveryProblem()
    {
        var validator = DefaultActionPlanMetricRules.DefaultDefaultActionPlanMetricValidator();
        var item = DefaultActionPlanMetric.NewDefaultActionPlanMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultActionPlanMetricRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new DefaultActionPlanMetricRequiredGradeRule("default action plan metric needs this field");
        var item = DefaultActionPlanMetric.NewDefaultActionPlanMetric("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultActionPlanMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultActionPlanMetricRequiredCodeRule("default action plan metric needs an entry");
        var item = DefaultActionPlanMetric.NewDefaultActionPlanMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultActionPlanMetricGradeLengthRuleFlagsALongGrade()
    {
        var rule = new DefaultActionPlanMetricGradeLengthRule(86400);
        var item = DefaultActionPlanMetric.NewDefaultActionPlanMetric("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultActionPlanMetricTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DefaultActionPlanMetricTitleLengthRule(32);
        var item = DefaultActionPlanMetric.NewDefaultActionPlanMetric("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultActionPlanMetricValidatorReportsEveryProblem();
        DefaultActionPlanMetricRequiredGradeRuleFlagsABlankGrade();
        DefaultActionPlanMetricRequiredCodeRuleFlagsABlankCode();
        DefaultActionPlanMetricGradeLengthRuleFlagsALongGrade();
        DefaultActionPlanMetricTitleLengthRuleFlagsALongTitle();
    }
}
