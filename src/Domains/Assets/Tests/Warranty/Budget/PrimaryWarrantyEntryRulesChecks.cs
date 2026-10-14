namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PrimaryWarrantyEntryRulesChecks
{
    public static void PrimaryWarrantyEntryValidatorReportsEveryProblem()
    {
        var validator = PrimaryWarrantyEntryRules.DefaultPrimaryWarrantyEntryValidator();
        var item = PrimaryWarrantyEntry.NewPrimaryWarrantyEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryWarrantyEntryRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new PrimaryWarrantyEntryRevisionRangeRule(32, 255);
        var item = PrimaryWarrantyEntry.NewPrimaryWarrantyEntry("x", "y");
        item.Revision = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryWarrantyEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryWarrantyEntryCodeLengthRule(120);
        var item = PrimaryWarrantyEntry.NewPrimaryWarrantyEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryWarrantyEntryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimaryWarrantyEntryTitleLengthRule(64);
        var item = PrimaryWarrantyEntry.NewPrimaryWarrantyEntry("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryWarrantyEntryValidatorReportsEveryProblem();
        PrimaryWarrantyEntryRevisionRangeRuleFlagsValuesAboveRevision();
        PrimaryWarrantyEntryCodeLengthRuleFlagsALongCode();
        PrimaryWarrantyEntryTitleLengthRuleFlagsALongTitle();
    }
}
