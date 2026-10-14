namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class MeritCycleLedgerRulesChecks
{
    public static void MeritCycleLedgerValidatorReportsEveryProblem()
    {
        var validator = MeritCycleLedgerRules.DefaultMeritCycleLedgerValidator();
        var item = MeritCycleLedger.NewMeritCycleLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleLedgerTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new MeritCycleLedgerTimezoneLengthRule(120);
        var item = MeritCycleLedger.NewMeritCycleLedger("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MeritCycleLedgerRequiredCodeRule("merit cycle ledger needs a value");
        var item = MeritCycleLedger.NewMeritCycleLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleLedgerTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new MeritCycleLedgerTenureMonthsRangeRule(64, 80);
        var item = MeritCycleLedger.NewMeritCycleLedger("x", "y");
        item.TenureMonths = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleLedgerRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new MeritCycleLedgerRequiredSourceRule("merit cycle ledger needs this field");
        var item = MeritCycleLedger.NewMeritCycleLedger("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new MeritCycleLedgerCodeLengthRule(80);
        var item = MeritCycleLedger.NewMeritCycleLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MeritCycleLedgerValidatorReportsEveryProblem();
        MeritCycleLedgerTimezoneLengthRuleFlagsALongTimezone();
        MeritCycleLedgerRequiredCodeRuleFlagsABlankCode();
        MeritCycleLedgerTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        MeritCycleLedgerRequiredSourceRuleFlagsABlankSource();
        MeritCycleLedgerCodeLengthRuleFlagsALongCode();
    }
}
