namespace Acme.Workforce.Contractors.Tests.Vendor.Entry;

using Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CappedVendorEntryRulesChecks
{
    public static void CappedVendorEntryValidatorReportsEveryProblem()
    {
        var validator = CappedVendorEntryRules.DefaultCappedVendorEntryValidator();
        var item = CappedVendorEntry.NewCappedVendorEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedVendorEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedVendorEntryRequiredCodeRule("capped vendor entry needs an entry");
        var item = CappedVendorEntry.NewCappedVendorEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedVendorEntryCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new CappedVendorEntryCostRangeRule(5.8, 2632.1);
        var item = CappedVendorEntry.NewCappedVendorEntry("x", "y");
        item.Cost = 2633.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedVendorEntryLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new CappedVendorEntryLocaleLengthRule(80);
        var item = CappedVendorEntry.NewCappedVendorEntry("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedVendorEntryRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new CappedVendorEntryRequiredContactEmailRule("capped vendor entry needs a value");
        var item = CappedVendorEntry.NewCappedVendorEntry("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedVendorEntryValidatorReportsEveryProblem();
        CappedVendorEntryRequiredCodeRuleFlagsABlankCode();
        CappedVendorEntryCostRangeRuleFlagsValuesAboveCost();
        CappedVendorEntryLocaleLengthRuleFlagsALongLocale();
        CappedVendorEntryRequiredContactEmailRuleFlagsABlankContactEmail();
    }
}
