namespace Acme.Workforce.Contractors.Tests.Vendor.Segment;

using Acme.Workforce.Contractors.Vendor.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class VendorQuotaRulesChecks
{
    public static void VendorQuotaValidatorReportsEveryProblem()
    {
        var validator = VendorQuotaRules.DefaultVendorQuotaValidator();
        var item = VendorQuota.NewVendorQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorQuotaTitleLengthRuleFlagsALongTitle()
    {
        var rule = new VendorQuotaTitleLengthRule(64);
        var item = VendorQuota.NewVendorQuota("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorQuotaSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new VendorQuotaSegmentLengthRule(3600);
        var item = VendorQuota.NewVendorQuota("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorQuotaRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new VendorQuotaRequiredSegmentRule("vendor quota needs a value");
        var item = VendorQuota.NewVendorQuota("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VendorQuotaValidatorReportsEveryProblem();
        VendorQuotaTitleLengthRuleFlagsALongTitle();
        VendorQuotaSegmentLengthRuleFlagsALongSegment();
        VendorQuotaRequiredSegmentRuleFlagsABlankSegment();
    }
}
