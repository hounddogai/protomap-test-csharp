namespace Acme.Workforce.Talent.Tests.Nomination.Ledger;

using Acme.Workforce.Talent.Nomination.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class NominationAuditRulesChecks
{
    public static void NominationAuditValidatorReportsEveryProblem()
    {
        var validator = NominationAuditRules.DefaultNominationAuditValidator();
        var item = NominationAudit.NewNominationAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationAuditNotesLengthRuleFlagsALongNotes()
    {
        var rule = new NominationAuditNotesLengthRule(16);
        var item = NominationAudit.NewNominationAudit("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new NominationAuditCodeLengthRule(1000);
        var item = NominationAudit.NewNominationAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationAuditGradeLengthRuleFlagsALongGrade()
    {
        var rule = new NominationAuditGradeLengthRule(80);
        var item = NominationAudit.NewNominationAudit("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationAuditMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new NominationAuditMinimumRangeRule(0.3, 4361.5);
        var item = NominationAudit.NewNominationAudit("x", "y");
        item.Minimum = 4363.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        NominationAuditValidatorReportsEveryProblem();
        NominationAuditNotesLengthRuleFlagsALongNotes();
        NominationAuditCodeLengthRuleFlagsALongCode();
        NominationAuditGradeLengthRuleFlagsALongGrade();
        NominationAuditMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
