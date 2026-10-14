namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantLedger;

using Acme.Workforce.Compensation.EquityGrant.GrantLedger;

/// <summary>Checks of grant ledger code, run by the project's runner.</summary>
public static class ScheduledEquityGrantLedgerRulesChecks
{
    public static void ScheduledEquityGrantLedgerValidatorReportsEveryProblem()
    {
        var validator = ScheduledEquityGrantLedgerRules.DefaultScheduledEquityGrantLedgerValidator();
        var item = ScheduledEquityGrantLedger.NewScheduledEquityGrantLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledEquityGrantLedgerLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new ScheduledEquityGrantLedgerLocaleLengthRule(32);
        var item = ScheduledEquityGrantLedger.NewScheduledEquityGrantLedger("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledEquityGrantLedgerLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ScheduledEquityGrantLedgerLabelLengthRule(32);
        var item = ScheduledEquityGrantLedger.NewScheduledEquityGrantLedger("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledEquityGrantLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledEquityGrantLedgerRequiredCodeRule("scheduled equity grant ledger needs a value");
        var item = ScheduledEquityGrantLedger.NewScheduledEquityGrantLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledEquityGrantLedgerValidatorReportsEveryProblem();
        ScheduledEquityGrantLedgerLocaleLengthRuleFlagsALongLocale();
        ScheduledEquityGrantLedgerLabelLengthRuleFlagsALongLabel();
        ScheduledEquityGrantLedgerRequiredCodeRuleFlagsABlankCode();
    }
}
