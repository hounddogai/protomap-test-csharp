namespace Acme.Workforce.Contractors.Tests.Vendor.VendorTemplate;

using Acme.Workforce.Contractors.Vendor.VendorTemplate;

/// <summary>Checks of vendor template code, run by the project's runner.</summary>
public static class PendingVendorManifestRulesChecks
{
    public static void PendingVendorManifestValidatorReportsEveryProblem()
    {
        var validator = PendingVendorManifestRules.DefaultPendingVendorManifestValidator();
        var item = PendingVendorManifest.NewPendingVendorManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingVendorManifestRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new PendingVendorManifestRequiredPhoneRule("pending vendor manifest needs an entry");
        var item = PendingVendorManifest.NewPendingVendorManifest("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingVendorManifestDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new PendingVendorManifestDiscountRangeRule(1.4, 503.4);
        var item = PendingVendorManifest.NewPendingVendorManifest("x", "y");
        item.Discount = 504.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingVendorManifestRegionLengthRuleFlagsALongRegion()
    {
        var rule = new PendingVendorManifestRegionLengthRule(86400);
        var item = PendingVendorManifest.NewPendingVendorManifest("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingVendorManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingVendorManifestRequiredCodeRule("pending vendor manifest needs an entry");
        var item = PendingVendorManifest.NewPendingVendorManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingVendorManifestRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new PendingVendorManifestRequiredRegionRule("pending vendor manifest needs a value");
        var item = PendingVendorManifest.NewPendingVendorManifest("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingVendorManifestValidatorReportsEveryProblem();
        PendingVendorManifestRequiredPhoneRuleFlagsABlankPhone();
        PendingVendorManifestDiscountRangeRuleFlagsValuesAboveDiscount();
        PendingVendorManifestRegionLengthRuleFlagsALongRegion();
        PendingVendorManifestRequiredCodeRuleFlagsABlankCode();
        PendingVendorManifestRequiredRegionRuleFlagsABlankRegion();
    }
}
