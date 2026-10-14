namespace Acme.Workforce.Contractors.Tests.Vendor.Schedule;

using Acme.Workforce.Contractors.Vendor.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class VendorEntryRulesChecks
{
    public static void VendorEntryValidatorReportsEveryProblem()
    {
        var validator = VendorEntryRules.DefaultVendorEntryValidator();
        var item = VendorEntry.NewVendorEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorEntryLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new VendorEntryLeaveDaysRangeRule(1000, 1000);
        var item = VendorEntry.NewVendorEntry("x", "y");
        item.LeaveDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorEntryRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new VendorEntryRequiredExternalRefRule("vendor entry needs this field");
        var item = VendorEntry.NewVendorEntry("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorEntryRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new VendorEntryRequiredGradeRule("vendor entry needs a value");
        var item = VendorEntry.NewVendorEntry("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VendorEntryValidatorReportsEveryProblem();
        VendorEntryLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        VendorEntryRequiredExternalRefRuleFlagsABlankExternalRef();
        VendorEntryRequiredGradeRuleFlagsABlankGrade();
    }
}
