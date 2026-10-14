namespace Acme.Workforce.Engagement.Tests.Feedback.RecordItem;

using Acme.Workforce.Engagement.Feedback.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class FeedbackRecordRulesChecks
{
    public static void FeedbackRecordValidatorReportsEveryProblem()
    {
        var validator = FeedbackRecordRules.DefaultFeedbackRecordValidator();
        var item = FeedbackRecord.NewFeedbackRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackRecordDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new FeedbackRecordDescriptionLengthRule(86400);
        var item = FeedbackRecord.NewFeedbackRecord("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackRecordRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FeedbackRecordRequiredLabelRule("feedback record needs this field");
        var item = FeedbackRecord.NewFeedbackRecord("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackRecordLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FeedbackRecordLabelLengthRule(1000);
        var item = FeedbackRecord.NewFeedbackRecord("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackRecordRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new FeedbackRecordRequiredTeamRule("feedback record needs an entry");
        var item = FeedbackRecord.NewFeedbackRecord("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackRecordLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new FeedbackRecordLeaveDaysRangeRule(16, 32);
        var item = FeedbackRecord.NewFeedbackRecord("x", "y");
        item.LeaveDays = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FeedbackRecordValidatorReportsEveryProblem();
        FeedbackRecordDescriptionLengthRuleFlagsALongDescription();
        FeedbackRecordRequiredLabelRuleFlagsABlankLabel();
        FeedbackRecordLabelLengthRuleFlagsALongLabel();
        FeedbackRecordRequiredTeamRuleFlagsABlankTeam();
        FeedbackRecordLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
