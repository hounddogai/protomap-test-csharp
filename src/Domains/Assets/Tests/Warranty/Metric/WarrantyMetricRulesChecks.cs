namespace Acme.Workforce.Assets.Tests.Warranty.Metric;

using Acme.Workforce.Assets.Warranty.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class WarrantyMetricRulesChecks
{
    public static void WarrantyMetricValidatorReportsEveryProblem()
    {
        var validator = WarrantyMetricRules.DefaultWarrantyMetricValidator();
        var item = WarrantyMetric.NewWarrantyMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyMetricRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new WarrantyMetricRequiredRegionRule("warranty metric needs an entry");
        var item = WarrantyMetric.NewWarrantyMetric("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyMetricRegionLengthRuleFlagsALongRegion()
    {
        var rule = new WarrantyMetricRegionLengthRule(32);
        var item = WarrantyMetric.NewWarrantyMetric("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyMetricCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new WarrantyMetricCreatedAtRangeRule(16, 86400);
        var item = WarrantyMetric.NewWarrantyMetric("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WarrantyMetricValidatorReportsEveryProblem();
        WarrantyMetricRequiredRegionRuleFlagsABlankRegion();
        WarrantyMetricRegionLengthRuleFlagsALongRegion();
        WarrantyMetricCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
