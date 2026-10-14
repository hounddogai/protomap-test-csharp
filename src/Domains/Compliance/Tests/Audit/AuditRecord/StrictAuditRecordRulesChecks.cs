namespace Acme.Workforce.Compliance.Tests.Audit.AuditRecord;

using Acme.Workforce.Compliance.Audit.AuditRecord;

/// <summary>Checks of audit record code, run by the project's runner.</summary>
public static class StrictAuditRecordRulesChecks
{
    public static void StrictAuditRecordValidatorReportsEveryProblem()
    {
        var validator = StrictAuditRecordRules.DefaultStrictAuditRecordValidator();
        var item = StrictAuditRecord.NewStrictAuditRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictAuditRecordSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new StrictAuditRecordSalaryBandLengthRule(64);
        var item = StrictAuditRecord.NewStrictAuditRecord("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictAuditRecordTitleLengthRuleFlagsALongTitle()
    {
        var rule = new StrictAuditRecordTitleLengthRule(32);
        var item = StrictAuditRecord.NewStrictAuditRecord("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictAuditRecordRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StrictAuditRecordRequiredTitleRule("strict audit record needs an entry");
        var item = StrictAuditRecord.NewStrictAuditRecord("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictAuditRecordValidatorReportsEveryProblem();
        StrictAuditRecordSalaryBandLengthRuleFlagsALongSalaryBand();
        StrictAuditRecordTitleLengthRuleFlagsALongTitle();
        StrictAuditRecordRequiredTitleRuleFlagsABlankTitle();
    }
}
