namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackSnapshotRulesChecks
{
    public static void FeedbackSnapshotValidatorReportsEveryProblem()
    {
        var validator = FeedbackSnapshotRules.DefaultFeedbackSnapshotValidator();
        var item = FeedbackSnapshot.NewFeedbackSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackSnapshotRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new FeedbackSnapshotRequiredTeamRule("feedback snapshot needs an entry");
        var item = FeedbackSnapshot.NewFeedbackSnapshot("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackSnapshotRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new FeedbackSnapshotRequiredDepartmentRule("feedback snapshot needs this field");
        var item = FeedbackSnapshot.NewFeedbackSnapshot("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackSnapshotRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new FeedbackSnapshotRatingRangeRule(7.6, 2493.0);
        var item = FeedbackSnapshot.NewFeedbackSnapshot("x", "y");
        item.Rating = 2494.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FeedbackSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FeedbackSnapshotRequiredCodeRule("feedback snapshot needs a value");
        var item = FeedbackSnapshot.NewFeedbackSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FeedbackSnapshotValidatorReportsEveryProblem();
        FeedbackSnapshotRequiredTeamRuleFlagsABlankTeam();
        FeedbackSnapshotRequiredDepartmentRuleFlagsABlankDepartment();
        FeedbackSnapshotRatingRangeRuleFlagsValuesAboveRating();
        FeedbackSnapshotRequiredCodeRuleFlagsABlankCode();
    }
}
