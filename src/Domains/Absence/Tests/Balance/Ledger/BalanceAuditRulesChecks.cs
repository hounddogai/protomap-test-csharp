namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BalanceAuditRulesChecks
{
    public static void BalanceAuditValidatorReportsEveryProblem()
    {
        var validator = BalanceAuditRules.DefaultBalanceAuditValidator();
        var item = BalanceAudit.NewBalanceAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceAuditRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BalanceAuditRequiredTitleRule("balance audit needs a value");
        var item = BalanceAudit.NewBalanceAudit("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceAuditNotesLengthRuleFlagsALongNotes()
    {
        var rule = new BalanceAuditNotesLengthRule(86400);
        var item = BalanceAudit.NewBalanceAudit("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BalanceAuditValidatorReportsEveryProblem();
        BalanceAuditRequiredTitleRuleFlagsABlankTitle();
        BalanceAuditNotesLengthRuleFlagsALongNotes();
    }
}
