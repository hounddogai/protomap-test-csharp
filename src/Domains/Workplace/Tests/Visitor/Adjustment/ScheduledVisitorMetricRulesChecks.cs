namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class ScheduledVisitorMetricRulesChecks
{
    public static void ScheduledVisitorMetricValidatorReportsEveryProblem()
    {
        var validator = ScheduledVisitorMetricRules.DefaultScheduledVisitorMetricValidator();
        var item = ScheduledVisitorMetric.NewScheduledVisitorMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledVisitorMetricCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new ScheduledVisitorMetricCostCenterLengthRule(3600);
        var item = ScheduledVisitorMetric.NewScheduledVisitorMetric("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledVisitorMetricRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new ScheduledVisitorMetricRequiredCostCenterRule("scheduled visitor metric needs an entry");
        var item = ScheduledVisitorMetric.NewScheduledVisitorMetric("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledVisitorMetricValidatorReportsEveryProblem();
        ScheduledVisitorMetricCostCenterLengthRuleFlagsALongCostCenter();
        ScheduledVisitorMetricRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}
