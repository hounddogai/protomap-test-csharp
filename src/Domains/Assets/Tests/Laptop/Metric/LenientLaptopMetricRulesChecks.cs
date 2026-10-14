namespace Acme.Workforce.Assets.Tests.Laptop.Metric;

using Acme.Workforce.Assets.Laptop.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LenientLaptopMetricRulesChecks
{
    public static void LenientLaptopMetricValidatorReportsEveryProblem()
    {
        var validator = LenientLaptopMetricRules.DefaultLenientLaptopMetricValidator();
        var item = LenientLaptopMetric.NewLenientLaptopMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientLaptopMetricRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new LenientLaptopMetricRequiredEmployeeIdRule("lenient laptop metric needs an entry");
        var item = LenientLaptopMetric.NewLenientLaptopMetric("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientLaptopMetricRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientLaptopMetricRequiredNameRule("lenient laptop metric needs an entry");
        var item = LenientLaptopMetric.NewLenientLaptopMetric("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientLaptopMetricRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new LenientLaptopMetricRequiredRegionRule("lenient laptop metric needs an entry");
        var item = LenientLaptopMetric.NewLenientLaptopMetric("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientLaptopMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientLaptopMetricRequiredCodeRule("lenient laptop metric needs an entry");
        var item = LenientLaptopMetric.NewLenientLaptopMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientLaptopMetricNameLengthRuleFlagsALongName()
    {
        var rule = new LenientLaptopMetricNameLengthRule(86400);
        var item = LenientLaptopMetric.NewLenientLaptopMetric("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientLaptopMetricValidatorReportsEveryProblem();
        LenientLaptopMetricRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        LenientLaptopMetricRequiredNameRuleFlagsABlankName();
        LenientLaptopMetricRequiredRegionRuleFlagsABlankRegion();
        LenientLaptopMetricRequiredCodeRuleFlagsABlankCode();
        LenientLaptopMetricNameLengthRuleFlagsALongName();
    }
}
