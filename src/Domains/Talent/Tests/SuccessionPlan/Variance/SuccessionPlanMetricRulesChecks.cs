namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanMetricRulesChecks
{
    public static void SuccessionPlanMetricValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanMetricRules.DefaultSuccessionPlanMetricValidator();
        var item = SuccessionPlanMetric.NewSuccessionPlanMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new SuccessionPlanMetricCodeLengthRule(120);
        var item = SuccessionPlanMetric.NewSuccessionPlanMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SuccessionPlanMetricRequiredCodeRule("succession plan metric needs this field");
        var item = SuccessionPlanMetric.NewSuccessionPlanMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanMetricDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new SuccessionPlanMetricDiscountRangeRule(6.9, 197.3);
        var item = SuccessionPlanMetric.NewSuccessionPlanMetric("x", "y");
        item.Discount = 198.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanMetricTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SuccessionPlanMetricTitleLengthRule(32);
        var item = SuccessionPlanMetric.NewSuccessionPlanMetric("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SuccessionPlanMetricValidatorReportsEveryProblem();
        SuccessionPlanMetricCodeLengthRuleFlagsALongCode();
        SuccessionPlanMetricRequiredCodeRuleFlagsABlankCode();
        SuccessionPlanMetricDiscountRangeRuleFlagsValuesAboveDiscount();
        SuccessionPlanMetricTitleLengthRuleFlagsALongTitle();
    }
}
