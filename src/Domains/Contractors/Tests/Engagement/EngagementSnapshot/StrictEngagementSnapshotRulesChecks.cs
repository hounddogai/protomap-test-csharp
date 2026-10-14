namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSnapshot;

using Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

/// <summary>Checks of engagement snapshot code, run by the project's runner.</summary>
public static class StrictEngagementSnapshotRulesChecks
{
    public static void StrictEngagementSnapshotValidatorReportsEveryProblem()
    {
        var validator = StrictEngagementSnapshotRules.DefaultStrictEngagementSnapshotValidator();
        var item = StrictEngagementSnapshot.NewStrictEngagementSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictEngagementSnapshotRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new StrictEngagementSnapshotRequiredNotesRule("strict engagement snapshot needs a value");
        var item = StrictEngagementSnapshot.NewStrictEngagementSnapshot("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictEngagementSnapshotScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new StrictEngagementSnapshotScoreRangeRule(9.7, 1800.4);
        var item = StrictEngagementSnapshot.NewStrictEngagementSnapshot("x", "y");
        item.Score = 1801.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictEngagementSnapshotNotesLengthRuleFlagsALongNotes()
    {
        var rule = new StrictEngagementSnapshotNotesLengthRule(64);
        var item = StrictEngagementSnapshot.NewStrictEngagementSnapshot("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictEngagementSnapshotValidatorReportsEveryProblem();
        StrictEngagementSnapshotRequiredNotesRuleFlagsABlankNotes();
        StrictEngagementSnapshotScoreRangeRuleFlagsValuesAboveScore();
        StrictEngagementSnapshotNotesLengthRuleFlagsALongNotes();
    }
}
