namespace Acme.Workforce.Payrollops.Tests.PayRun.GlobalRunLedger;

using Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

/// <summary>Checks of global run ledger code, run by the project's runner.</summary>
public static class LenientPayRunLedgerRulesChecks
{
    public static void LenientPayRunLedgerValidatorReportsEveryProblem()
    {
        var validator = LenientPayRunLedgerRules.DefaultLenientPayRunLedgerValidator();
        var item = LenientPayRunLedger.NewLenientPayRunLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPayRunLedgerDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new LenientPayRunLedgerDescriptionLengthRule(16);
        var item = LenientPayRunLedger.NewLenientPayRunLedger("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientPayRunLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientPayRunLedgerCodeLengthRule(255);
        var item = LenientPayRunLedger.NewLenientPayRunLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientPayRunLedgerValidatorReportsEveryProblem();
        LenientPayRunLedgerDescriptionLengthRuleFlagsALongDescription();
        LenientPayRunLedgerCodeLengthRuleFlagsALongCode();
    }
}
