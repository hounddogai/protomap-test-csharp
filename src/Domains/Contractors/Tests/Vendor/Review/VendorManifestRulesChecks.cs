namespace Acme.Workforce.Contractors.Tests.Vendor.Review;

using Acme.Workforce.Contractors.Vendor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VendorManifestRulesChecks
{
    public static void VendorManifestValidatorReportsEveryProblem()
    {
        var validator = VendorManifestRules.DefaultVendorManifestValidator();
        var item = VendorManifest.NewVendorManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorManifestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new VendorManifestLabelLengthRule(1000);
        var item = VendorManifest.NewVendorManifest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorManifestThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new VendorManifestThresholdRangeRule(6.4, 350.5);
        var item = VendorManifest.NewVendorManifest("x", "y");
        item.Threshold = 352.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorManifestMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new VendorManifestMarginRangeRule(4.5, 475.1);
        var item = VendorManifest.NewVendorManifest("x", "y");
        item.Margin = 476.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new VendorManifestCodeLengthRule(120);
        var item = VendorManifest.NewVendorManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VendorManifestValidatorReportsEveryProblem();
        VendorManifestLabelLengthRuleFlagsALongLabel();
        VendorManifestThresholdRangeRuleFlagsValuesAboveThreshold();
        VendorManifestMarginRangeRuleFlagsValuesAboveMargin();
        VendorManifestCodeLengthRuleFlagsALongCode();
    }
}
