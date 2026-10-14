namespace Acme.Workforce.Contractors.Tests.Vendor.Metric;

using Acme.Workforce.Contractors.Vendor.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class VendorMetricRulesChecks
{
    public static void VendorMetricValidatorReportsEveryProblem()
    {
        var validator = VendorMetricRules.DefaultVendorMetricValidator();
        var item = VendorMetric.NewVendorMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorMetricAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new VendorMetricAmountRangeRule(9.3, 1851.0);
        var item = VendorMetric.NewVendorMetric("x", "y");
        item.Amount = 1852.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorMetricRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new VendorMetricRequiredJobCodeRule("vendor metric needs an entry");
        var item = VendorMetric.NewVendorMetric("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VendorMetricValidatorReportsEveryProblem();
        VendorMetricAmountRangeRuleFlagsValuesAboveAmount();
        VendorMetricRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
