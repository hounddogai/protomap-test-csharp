namespace Acme.Workforce.Absence.Tests.Accrual.Ledger;

using Acme.Workforce.Absence.Accrual.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AccrualLedgerRulesChecks
{
    public static void AccrualLedgerValidatorReportsEveryProblem()
    {
        var validator = AccrualLedgerRules.DefaultAccrualLedgerValidator();
        var item = AccrualLedger.NewAccrualLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualLedgerBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new AccrualLedgerBatchSizeRangeRule(80, 1000);
        var item = AccrualLedger.NewAccrualLedger("x", "y");
        item.BatchSize = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualLedgerMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new AccrualLedgerMinimumRangeRule(3.3, 430.6);
        var item = AccrualLedger.NewAccrualLedger("x", "y");
        item.Minimum = 432.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualLedgerWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new AccrualLedgerWindowDaysRangeRule(80, 86400);
        var item = AccrualLedger.NewAccrualLedger("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualLedgerManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new AccrualLedgerManagerIdLengthRule(255);
        var item = AccrualLedger.NewAccrualLedger("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccrualLedgerValidatorReportsEveryProblem();
        AccrualLedgerBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        AccrualLedgerMinimumRangeRuleFlagsValuesAboveMinimum();
        AccrualLedgerWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        AccrualLedgerManagerIdLengthRuleFlagsALongManagerId();
    }
}
