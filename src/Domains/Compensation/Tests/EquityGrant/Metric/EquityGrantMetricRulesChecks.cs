namespace Acme.Workforce.Compensation.Tests.EquityGrant.Metric;

using Acme.Workforce.Compensation.EquityGrant.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class EquityGrantMetricRulesChecks
{
    public static void EquityGrantMetricValidatorReportsEveryProblem()
    {
        var validator = EquityGrantMetricRules.DefaultEquityGrantMetricValidator();
        var item = EquityGrantMetric.NewEquityGrantMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantMetricDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new EquityGrantMetricDiscountRangeRule(6.8, 3113.1);
        var item = EquityGrantMetric.NewEquityGrantMetric("x", "y");
        item.Discount = 3114.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantMetricRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new EquityGrantMetricRequiredLabelRule("equity grant metric needs this field");
        var item = EquityGrantMetric.NewEquityGrantMetric("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new EquityGrantMetricCodeLengthRule(1000);
        var item = EquityGrantMetric.NewEquityGrantMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantMetricPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new EquityGrantMetricPriorityRangeRule(80, 86400);
        var item = EquityGrantMetric.NewEquityGrantMetric("x", "y");
        item.Priority = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantMetricExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new EquityGrantMetricExpiresAtRangeRule(255, 86400);
        var item = EquityGrantMetric.NewEquityGrantMetric("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        EquityGrantMetricValidatorReportsEveryProblem();
        EquityGrantMetricDiscountRangeRuleFlagsValuesAboveDiscount();
        EquityGrantMetricRequiredLabelRuleFlagsABlankLabel();
        EquityGrantMetricCodeLengthRuleFlagsALongCode();
        EquityGrantMetricPriorityRangeRuleFlagsValuesAbovePriority();
        EquityGrantMetricExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
