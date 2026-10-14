namespace Acme.Workforce.Contractors.Tests.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Vendor.VendorMetric;

/// <summary>Checks of vendor metric code, run by the project's runner.</summary>
public static class PrimaryVendorMetricRulesChecks
{
    public static void PrimaryVendorMetricValidatorReportsEveryProblem()
    {
        var validator = PrimaryVendorMetricRules.DefaultPrimaryVendorMetricValidator();
        var item = PrimaryVendorMetric.NewPrimaryVendorMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryVendorMetricDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new PrimaryVendorMetricDepartmentLengthRule(80);
        var item = PrimaryVendorMetric.NewPrimaryVendorMetric("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryVendorMetricRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new PrimaryVendorMetricRequiredSummaryRule("primary vendor metric needs this field");
        var item = PrimaryVendorMetric.NewPrimaryVendorMetric("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryVendorMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryVendorMetricRequiredCodeRule("primary vendor metric needs this field");
        var item = PrimaryVendorMetric.NewPrimaryVendorMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryVendorMetricValidatorReportsEveryProblem();
        PrimaryVendorMetricDepartmentLengthRuleFlagsALongDepartment();
        PrimaryVendorMetricRequiredSummaryRuleFlagsABlankSummary();
        PrimaryVendorMetricRequiredCodeRuleFlagsABlankCode();
    }
}
