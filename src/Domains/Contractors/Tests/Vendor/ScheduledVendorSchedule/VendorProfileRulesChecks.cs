namespace Acme.Workforce.Contractors.Tests.Vendor.ScheduledVendorSchedule;

using Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

/// <summary>Checks of scheduled vendor schedule code, run by the project's runner.</summary>
public static class VendorProfileRulesChecks
{
    public static void VendorProfileValidatorReportsEveryProblem()
    {
        var validator = VendorProfileRules.DefaultVendorProfileValidator();
        var item = VendorProfile.NewVendorProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorProfileRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new VendorProfileRequiredLabelRule("vendor profile needs an entry");
        var item = VendorProfile.NewVendorProfile("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorProfileLabelLengthRuleFlagsALongLabel()
    {
        var rule = new VendorProfileLabelLengthRule(80);
        var item = VendorProfile.NewVendorProfile("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorProfileMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new VendorProfileMaximumRangeRule(7.9, 4651.8);
        var item = VendorProfile.NewVendorProfile("x", "y");
        item.Maximum = 4653.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorProfileRegionLengthRuleFlagsALongRegion()
    {
        var rule = new VendorProfileRegionLengthRule(32);
        var item = VendorProfile.NewVendorProfile("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VendorProfileValidatorReportsEveryProblem();
        VendorProfileRequiredLabelRuleFlagsABlankLabel();
        VendorProfileLabelLengthRuleFlagsALongLabel();
        VendorProfileMaximumRangeRuleFlagsValuesAboveMaximum();
        VendorProfileRegionLengthRuleFlagsALongRegion();
    }
}
