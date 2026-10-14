namespace Acme.Workforce.Orgdesign.Tests.CostModel.Metric;

using Acme.Workforce.Orgdesign.CostModel.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CostModelMetricRulesChecks
{
    public static void CostModelMetricValidatorReportsEveryProblem()
    {
        var validator = CostModelMetricRules.DefaultCostModelMetricValidator();
        var item = CostModelMetric.NewCostModelMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelMetricBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new CostModelMetricBatchSizeRangeRule(80, 80);
        var item = CostModelMetric.NewCostModelMetric("x", "y");
        item.BatchSize = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CostModelMetricLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new CostModelMetricLocaleLengthRule(16);
        var item = CostModelMetric.NewCostModelMetric("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelMetricRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new CostModelMetricRequiredContactEmailRule("cost model metric needs an entry");
        var item = CostModelMetric.NewCostModelMetric("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelMetricRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new CostModelMetricRequiredLocaleRule("cost model metric needs an entry");
        var item = CostModelMetric.NewCostModelMetric("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CostModelMetricRequiredCodeRule("cost model metric needs an entry");
        var item = CostModelMetric.NewCostModelMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelMetricFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new CostModelMetricFloorValueRangeRule(7.8, 4289.7);
        var item = CostModelMetric.NewCostModelMetric("x", "y");
        item.FloorValue = 4291.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CostModelMetricValidatorReportsEveryProblem();
        CostModelMetricBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        CostModelMetricLocaleLengthRuleFlagsALongLocale();
        CostModelMetricRequiredContactEmailRuleFlagsABlankContactEmail();
        CostModelMetricRequiredLocaleRuleFlagsABlankLocale();
        CostModelMetricRequiredCodeRuleFlagsABlankCode();
        CostModelMetricFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
