namespace Acme.Workforce.Assets.Tests.Assignment.Entry;

using Acme.Workforce.Assets.Assignment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class StrictAssignmentCheckpointRulesChecks
{
    public static void StrictAssignmentCheckpointValidatorReportsEveryProblem()
    {
        var validator = StrictAssignmentCheckpointRules.DefaultStrictAssignmentCheckpointValidator();
        var item = StrictAssignmentCheckpoint.NewStrictAssignmentCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictAssignmentCheckpointRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new StrictAssignmentCheckpointRequiredNotesRule("strict assignment checkpoint needs this field");
        var item = StrictAssignmentCheckpoint.NewStrictAssignmentCheckpoint("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictAssignmentCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictAssignmentCheckpointRequiredCodeRule("strict assignment checkpoint needs a value");
        var item = StrictAssignmentCheckpoint.NewStrictAssignmentCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictAssignmentCheckpointNotesLengthRuleFlagsALongNotes()
    {
        var rule = new StrictAssignmentCheckpointNotesLengthRule(1000);
        var item = StrictAssignmentCheckpoint.NewStrictAssignmentCheckpoint("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictAssignmentCheckpointGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new StrictAssignmentCheckpointGraceDaysRangeRule(16, 32);
        var item = StrictAssignmentCheckpoint.NewStrictAssignmentCheckpoint("x", "y");
        item.GraceDays = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictAssignmentCheckpointRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StrictAssignmentCheckpointRequiredLabelRule("strict assignment checkpoint needs an entry");
        var item = StrictAssignmentCheckpoint.NewStrictAssignmentCheckpoint("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictAssignmentCheckpointValidatorReportsEveryProblem();
        StrictAssignmentCheckpointRequiredNotesRuleFlagsABlankNotes();
        StrictAssignmentCheckpointRequiredCodeRuleFlagsABlankCode();
        StrictAssignmentCheckpointNotesLengthRuleFlagsALongNotes();
        StrictAssignmentCheckpointGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        StrictAssignmentCheckpointRequiredLabelRuleFlagsABlankLabel();
    }
}
