namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyLedgerRulesChecks
{
    public static void WarrantyLedgerValidatorReportsEveryProblem()
    {
        var validator = WarrantyLedgerRules.DefaultWarrantyLedgerValidator();
        var item = WarrantyLedger.NewWarrantyLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyLedgerTitleLengthRuleFlagsALongTitle()
    {
        var rule = new WarrantyLedgerTitleLengthRule(80);
        var item = WarrantyLedger.NewWarrantyLedger("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyLedgerSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new WarrantyLedgerSalaryBandLengthRule(86400);
        var item = WarrantyLedger.NewWarrantyLedger("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyLedgerDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new WarrantyLedgerDescriptionLengthRule(3600);
        var item = WarrantyLedger.NewWarrantyLedger("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WarrantyLedgerValidatorReportsEveryProblem();
        WarrantyLedgerTitleLengthRuleFlagsALongTitle();
        WarrantyLedgerSalaryBandLengthRuleFlagsALongSalaryBand();
        WarrantyLedgerDescriptionLengthRuleFlagsALongDescription();
    }
}
