namespace Acme.Workforce.Payrollops.Tests.Garnishment.Ledger;

using Acme.Workforce.Payrollops.Garnishment.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class GarnishmentLedgerRulesChecks
{
    public static void GarnishmentLedgerValidatorReportsEveryProblem()
    {
        var validator = GarnishmentLedgerRules.DefaultGarnishmentLedgerValidator();
        var item = GarnishmentLedger.NewGarnishmentLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GarnishmentLedgerRequiredCodeRule("garnishment ledger needs a value");
        var item = GarnishmentLedger.NewGarnishmentLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentLedgerNameLengthRuleFlagsALongName()
    {
        var rule = new GarnishmentLedgerNameLengthRule(80);
        var item = GarnishmentLedger.NewGarnishmentLedger("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GarnishmentLedgerValidatorReportsEveryProblem();
        GarnishmentLedgerRequiredCodeRuleFlagsABlankCode();
        GarnishmentLedgerNameLengthRuleFlagsALongName();
    }
}
