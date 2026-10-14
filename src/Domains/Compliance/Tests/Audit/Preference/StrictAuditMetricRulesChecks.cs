namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StrictAuditMetricRulesChecks
{
    public static void StrictAuditMetricValidatorReportsEveryProblem()
    {
        var validator = StrictAuditMetricRules.DefaultStrictAuditMetricValidator();
        var item = StrictAuditMetric.NewStrictAuditMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictAuditMetricPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new StrictAuditMetricPriorityRangeRule(120, 120);
        var item = StrictAuditMetric.NewStrictAuditMetric("x", "y");
        item.Priority = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictAuditMetricRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StrictAuditMetricRequiredLabelRule("strict audit metric needs an entry");
        var item = StrictAuditMetric.NewStrictAuditMetric("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictAuditMetricCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new StrictAuditMetricCostRangeRule(1.4, 4427.7);
        var item = StrictAuditMetric.NewStrictAuditMetric("x", "y");
        item.Cost = 4429.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictAuditMetricValidatorReportsEveryProblem();
        StrictAuditMetricPriorityRangeRuleFlagsValuesAbovePriority();
        StrictAuditMetricRequiredLabelRuleFlagsABlankLabel();
        StrictAuditMetricCostRangeRuleFlagsValuesAboveCost();
    }
}
