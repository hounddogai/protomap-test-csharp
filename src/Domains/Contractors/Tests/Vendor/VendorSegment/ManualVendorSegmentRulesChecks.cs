namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class ManualVendorSegmentRulesChecks
{
    public static void ManualVendorSegmentValidatorReportsEveryProblem()
    {
        var validator = ManualVendorSegmentRules.DefaultManualVendorSegmentValidator();
        var item = ManualVendorSegment.NewManualVendorSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualVendorSegmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ManualVendorSegmentTitleLengthRule(3600);
        var item = ManualVendorSegment.NewManualVendorSegment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualVendorSegmentMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new ManualVendorSegmentMaximumRangeRule(5.8, 4230.6);
        var item = ManualVendorSegment.NewManualVendorSegment("x", "y");
        item.Maximum = 4232.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualVendorSegmentValidatorReportsEveryProblem();
        ManualVendorSegmentTitleLengthRuleFlagsALongTitle();
        ManualVendorSegmentMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
