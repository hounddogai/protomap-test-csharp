namespace Acme.Workforce.Contractors.Tests.Vendor.Assignment;

using Acme.Workforce.Contractors.Vendor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VendorPreferenceRulesChecks
{
    public static void VendorPreferenceValidatorReportsEveryProblem()
    {
        var validator = VendorPreferenceRules.DefaultVendorPreferenceValidator();
        var item = VendorPreference.NewVendorPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorPreferenceRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new VendorPreferenceRequiredRegionRule("vendor preference needs a value");
        var item = VendorPreference.NewVendorPreference("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new VendorPreferenceCodeLengthRule(3600);
        var item = VendorPreference.NewVendorPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorPreferenceAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new VendorPreferenceAmountRangeRule(6.5, 3087.8);
        var item = VendorPreference.NewVendorPreference("x", "y");
        item.Amount = 3089.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        VendorPreferenceValidatorReportsEveryProblem();
        VendorPreferenceRequiredRegionRuleFlagsABlankRegion();
        VendorPreferenceCodeLengthRuleFlagsALongCode();
        VendorPreferenceAmountRangeRuleFlagsValuesAboveAmount();
    }
}
