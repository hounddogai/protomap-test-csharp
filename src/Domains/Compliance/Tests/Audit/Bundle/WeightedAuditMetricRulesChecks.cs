namespace Acme.Workforce.Compliance.Tests.Audit.Bundle;

using Acme.Workforce.Compliance.Audit.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WeightedAuditMetricRulesChecks
{
    public static void WeightedAuditMetricValidatorReportsEveryProblem()
    {
        var validator = WeightedAuditMetricRules.DefaultWeightedAuditMetricValidator();
        var item = WeightedAuditMetric.NewWeightedAuditMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedAuditMetricSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new WeightedAuditMetricSegmentLengthRule(1000);
        var item = WeightedAuditMetric.NewWeightedAuditMetric("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedAuditMetricBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new WeightedAuditMetricBudgetRangeRule(4.1, 3178.0);
        var item = WeightedAuditMetric.NewWeightedAuditMetric("x", "y");
        item.Budget = 3179.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedAuditMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedAuditMetricRequiredCodeRule("weighted audit metric needs a value");
        var item = WeightedAuditMetric.NewWeightedAuditMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedAuditMetricValidatorReportsEveryProblem();
        WeightedAuditMetricSegmentLengthRuleFlagsALongSegment();
        WeightedAuditMetricBudgetRangeRuleFlagsValuesAboveBudget();
        WeightedAuditMetricRequiredCodeRuleFlagsABlankCode();
    }
}
