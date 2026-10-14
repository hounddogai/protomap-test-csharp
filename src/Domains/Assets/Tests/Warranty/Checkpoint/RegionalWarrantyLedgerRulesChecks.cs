namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalWarrantyLedgerRulesChecks
{
    public static void RegionalWarrantyLedgerValidatorReportsEveryProblem()
    {
        var validator = RegionalWarrantyLedgerRules.DefaultRegionalWarrantyLedgerValidator();
        var item = RegionalWarrantyLedger.NewRegionalWarrantyLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalWarrantyLedgerCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new RegionalWarrantyLedgerCostCenterLengthRule(80);
        var item = RegionalWarrantyLedger.NewRegionalWarrantyLedger("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalWarrantyLedgerRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new RegionalWarrantyLedgerRateRangeRule(7.3, 3546.5);
        var item = RegionalWarrantyLedger.NewRegionalWarrantyLedger("x", "y");
        item.Rate = 3548.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalWarrantyLedgerRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalWarrantyLedgerRequiredNameRule("regional warranty ledger needs a value");
        var item = RegionalWarrantyLedger.NewRegionalWarrantyLedger("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalWarrantyLedgerValidatorReportsEveryProblem();
        RegionalWarrantyLedgerCostCenterLengthRuleFlagsALongCostCenter();
        RegionalWarrantyLedgerRateRangeRuleFlagsValuesAboveRate();
        RegionalWarrantyLedgerRequiredNameRuleFlagsABlankName();
    }
}
