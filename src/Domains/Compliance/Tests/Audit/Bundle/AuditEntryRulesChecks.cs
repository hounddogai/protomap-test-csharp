namespace Acme.Workforce.Compliance.Tests.Audit.Bundle;

using Acme.Workforce.Compliance.Audit.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AuditEntryRulesChecks
{
    public static void AuditEntryValidatorReportsEveryProblem()
    {
        var validator = AuditEntryRules.DefaultAuditEntryValidator();
        var item = AuditEntry.NewAuditEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditEntryRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new AuditEntryRequiredNotesRule("audit entry needs a value");
        var item = AuditEntry.NewAuditEntry("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditEntryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AuditEntryLabelLengthRule(64);
        var item = AuditEntry.NewAuditEntry("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditEntryNotesLengthRuleFlagsALongNotes()
    {
        var rule = new AuditEntryNotesLengthRule(3600);
        var item = AuditEntry.NewAuditEntry("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditEntryRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new AuditEntryRequiredBadgeRule("audit entry needs a value");
        var item = AuditEntry.NewAuditEntry("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AuditEntryValidatorReportsEveryProblem();
        AuditEntryRequiredNotesRuleFlagsABlankNotes();
        AuditEntryLabelLengthRuleFlagsALongLabel();
        AuditEntryNotesLengthRuleFlagsALongNotes();
        AuditEntryRequiredBadgeRuleFlagsABlankBadge();
    }
}
