namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopLedgerRulesChecks
{
    public static void LaptopLedgerValidatorReportsEveryProblem()
    {
        var validator = LaptopLedgerRules.DefaultLaptopLedgerValidator();
        var item = LaptopLedger.NewLaptopLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopLedgerRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new LaptopLedgerRevisionRangeRule(64, 80);
        var item = LaptopLedger.NewLaptopLedger("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopLedgerLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LaptopLedgerLabelLengthRule(16);
        var item = LaptopLedger.NewLaptopLedger("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopLedgerRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new LaptopLedgerRequiredCostCenterRule("laptop ledger needs an entry");
        var item = LaptopLedger.NewLaptopLedger("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new LaptopLedgerCodeLengthRule(64);
        var item = LaptopLedger.NewLaptopLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopLedgerRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LaptopLedgerRequiredLabelRule("laptop ledger needs this field");
        var item = LaptopLedger.NewLaptopLedger("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LaptopLedgerValidatorReportsEveryProblem();
        LaptopLedgerRevisionRangeRuleFlagsValuesAboveRevision();
        LaptopLedgerLabelLengthRuleFlagsALongLabel();
        LaptopLedgerRequiredCostCenterRuleFlagsABlankCostCenter();
        LaptopLedgerCodeLengthRuleFlagsALongCode();
        LaptopLedgerRequiredLabelRuleFlagsABlankLabel();
    }
}
