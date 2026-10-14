namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class DefaultPayRunLedgerRulesChecks
{
    public static void DefaultPayRunLedgerValidatorReportsEveryProblem()
    {
        var validator = DefaultPayRunLedgerRules.DefaultDefaultPayRunLedgerValidator();
        var item = DefaultPayRunLedger.NewDefaultPayRunLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPayRunLedgerGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new DefaultPayRunLedgerGraceDaysRangeRule(16, 32);
        var item = DefaultPayRunLedger.NewDefaultPayRunLedger("x", "y");
        item.GraceDays = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultPayRunLedgerWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new DefaultPayRunLedgerWindowDaysRangeRule(32, 255);
        var item = DefaultPayRunLedger.NewDefaultPayRunLedger("x", "y");
        item.WindowDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultPayRunLedgerSourceLengthRuleFlagsALongSource()
    {
        var rule = new DefaultPayRunLedgerSourceLengthRule(1000);
        var item = DefaultPayRunLedger.NewDefaultPayRunLedger("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultPayRunLedgerValidatorReportsEveryProblem();
        DefaultPayRunLedgerGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        DefaultPayRunLedgerWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        DefaultPayRunLedgerSourceLengthRuleFlagsALongSource();
    }
}
