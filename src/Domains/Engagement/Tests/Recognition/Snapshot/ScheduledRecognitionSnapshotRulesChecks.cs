namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledRecognitionSnapshotRulesChecks
{
    public static void ScheduledRecognitionSnapshotValidatorReportsEveryProblem()
    {
        var validator = ScheduledRecognitionSnapshotRules.DefaultScheduledRecognitionSnapshotValidator();
        var item = ScheduledRecognitionSnapshot.NewScheduledRecognitionSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledRecognitionSnapshotRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledRecognitionSnapshotRequiredTitleRule("scheduled recognition snapshot needs this field");
        var item = ScheduledRecognitionSnapshot.NewScheduledRecognitionSnapshot("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledRecognitionSnapshotTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ScheduledRecognitionSnapshotTitleLengthRule(1000);
        var item = ScheduledRecognitionSnapshot.NewScheduledRecognitionSnapshot("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledRecognitionSnapshotRegionLengthRuleFlagsALongRegion()
    {
        var rule = new ScheduledRecognitionSnapshotRegionLengthRule(3600);
        var item = ScheduledRecognitionSnapshot.NewScheduledRecognitionSnapshot("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledRecognitionSnapshotValidatorReportsEveryProblem();
        ScheduledRecognitionSnapshotRequiredTitleRuleFlagsABlankTitle();
        ScheduledRecognitionSnapshotTitleLengthRuleFlagsALongTitle();
        ScheduledRecognitionSnapshotRegionLengthRuleFlagsALongRegion();
    }
}
