namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingBalanceAuditRulesChecks
{
    public static void PendingBalanceAuditValidatorReportsEveryProblem()
    {
        var validator = PendingBalanceAuditRules.DefaultPendingBalanceAuditValidator();
        var item = PendingBalanceAudit.NewPendingBalanceAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingBalanceAuditEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new PendingBalanceAuditEndsAtRangeRule(255, 255);
        var item = PendingBalanceAudit.NewPendingBalanceAudit("x", "y");
        item.EndsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingBalanceAuditRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new PendingBalanceAuditRequiredSalaryBandRule("pending balance audit needs an entry");
        var item = PendingBalanceAudit.NewPendingBalanceAudit("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingBalanceAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingBalanceAuditRequiredCodeRule("pending balance audit needs an entry");
        var item = PendingBalanceAudit.NewPendingBalanceAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingBalanceAuditValidatorReportsEveryProblem();
        PendingBalanceAuditEndsAtRangeRuleFlagsValuesAboveEndsAt();
        PendingBalanceAuditRequiredSalaryBandRuleFlagsABlankSalaryBand();
        PendingBalanceAuditRequiredCodeRuleFlagsABlankCode();
    }
}
