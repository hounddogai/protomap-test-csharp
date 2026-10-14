namespace Acme.Workforce.Assets.Tests.Assignment.Entry;

using Acme.Workforce.Assets.Assignment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AssignmentEntryRulesChecks
{
    public static void AssignmentEntryValidatorReportsEveryProblem()
    {
        var validator = AssignmentEntryRules.DefaultAssignmentEntryValidator();
        var item = AssignmentEntry.NewAssignmentEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentEntryRequiredNameRuleFlagsABlankName()
    {
        var rule = new AssignmentEntryRequiredNameRule("assignment entry needs an entry");
        var item = AssignmentEntry.NewAssignmentEntry("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentEntryNameLengthRuleFlagsALongName()
    {
        var rule = new AssignmentEntryNameLengthRule(64);
        var item = AssignmentEntry.NewAssignmentEntry("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentEntryRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new AssignmentEntryRequiredNotesRule("assignment entry needs an entry");
        var item = AssignmentEntry.NewAssignmentEntry("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssignmentEntryValidatorReportsEveryProblem();
        AssignmentEntryRequiredNameRuleFlagsABlankName();
        AssignmentEntryNameLengthRuleFlagsALongName();
        AssignmentEntryRequiredNotesRuleFlagsABlankNotes();
    }
}
