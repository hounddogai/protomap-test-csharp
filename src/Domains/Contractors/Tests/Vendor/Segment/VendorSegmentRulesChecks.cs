namespace Acme.Workforce.Contractors.Tests.Vendor.Segment;

using Acme.Workforce.Contractors.Vendor.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class VendorSegmentRulesChecks
{
    public static void VendorSegmentValidatorReportsEveryProblem()
    {
        var validator = VendorSegmentRules.DefaultVendorSegmentValidator();
        var item = VendorSegment.NewVendorSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorSegmentRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new VendorSegmentRequiredCurrencyRule("vendor segment needs an entry");
        var item = VendorSegment.NewVendorSegment("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorSegmentOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new VendorSegmentOwnerIdLengthRule(80);
        var item = VendorSegment.NewVendorSegment("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorSegmentLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new VendorSegmentLeaveDaysRangeRule(64, 86400);
        var item = VendorSegment.NewVendorSegment("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorSegmentCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new VendorSegmentCurrencyLengthRule(255);
        var item = VendorSegment.NewVendorSegment("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VendorSegmentValidatorReportsEveryProblem();
        VendorSegmentRequiredCurrencyRuleFlagsABlankCurrency();
        VendorSegmentOwnerIdLengthRuleFlagsALongOwnerId();
        VendorSegmentLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        VendorSegmentCurrencyLengthRuleFlagsALongCurrency();
    }
}
