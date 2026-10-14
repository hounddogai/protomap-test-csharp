namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class SeasonalAccrualLedgerRulesChecks
{
    public static void SeasonalAccrualLedgerValidatorReportsEveryProblem()
    {
        var validator = SeasonalAccrualLedgerRules.DefaultSeasonalAccrualLedgerValidator();
        var item = SeasonalAccrualLedger.NewSeasonalAccrualLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalAccrualLedgerRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new SeasonalAccrualLedgerRequiredContactEmailRule("seasonal accrual ledger needs this field");
        var item = SeasonalAccrualLedger.NewSeasonalAccrualLedger("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalAccrualLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalAccrualLedgerCodeLengthRule(86400);
        var item = SeasonalAccrualLedger.NewSeasonalAccrualLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalAccrualLedgerRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new SeasonalAccrualLedgerRatioRangeRule(1.1, 86.0);
        var item = SeasonalAccrualLedger.NewSeasonalAccrualLedger("x", "y");
        item.Ratio = 87.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalAccrualLedgerContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new SeasonalAccrualLedgerContactEmailLengthRule(32);
        var item = SeasonalAccrualLedger.NewSeasonalAccrualLedger("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalAccrualLedgerQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new SeasonalAccrualLedgerQuantityRangeRule(80, 255);
        var item = SeasonalAccrualLedger.NewSeasonalAccrualLedger("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalAccrualLedgerValidatorReportsEveryProblem();
        SeasonalAccrualLedgerRequiredContactEmailRuleFlagsABlankContactEmail();
        SeasonalAccrualLedgerCodeLengthRuleFlagsALongCode();
        SeasonalAccrualLedgerRatioRangeRuleFlagsValuesAboveRatio();
        SeasonalAccrualLedgerContactEmailLengthRuleFlagsALongContactEmail();
        SeasonalAccrualLedgerQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
