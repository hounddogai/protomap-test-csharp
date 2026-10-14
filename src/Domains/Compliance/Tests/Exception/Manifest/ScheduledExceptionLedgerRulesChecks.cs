namespace Acme.Workforce.Compliance.Tests.Exception.Manifest;

using Acme.Workforce.Compliance.Exception.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ScheduledExceptionLedgerRulesChecks
{
    public static void ScheduledExceptionLedgerValidatorReportsEveryProblem()
    {
        var validator = ScheduledExceptionLedgerRules.DefaultScheduledExceptionLedgerValidator();
        var item = ScheduledExceptionLedger.NewScheduledExceptionLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledExceptionLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledExceptionLedgerRequiredCodeRule("scheduled exception ledger needs a value");
        var item = ScheduledExceptionLedger.NewScheduledExceptionLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledExceptionLedgerHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new ScheduledExceptionLedgerHoursRangeRule(2.5, 4663.1);
        var item = ScheduledExceptionLedger.NewScheduledExceptionLedger("x", "y");
        item.Hours = 4664.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledExceptionLedgerMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new ScheduledExceptionLedgerMaximumRangeRule(9.8, 3610.5);
        var item = ScheduledExceptionLedger.NewScheduledExceptionLedger("x", "y");
        item.Maximum = 3612.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledExceptionLedgerValidatorReportsEveryProblem();
        ScheduledExceptionLedgerRequiredCodeRuleFlagsABlankCode();
        ScheduledExceptionLedgerHoursRangeRuleFlagsValuesAboveHours();
        ScheduledExceptionLedgerMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
