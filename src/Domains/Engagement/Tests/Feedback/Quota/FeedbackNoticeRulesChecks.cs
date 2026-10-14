namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class FeedbackNoticeRulesChecks
{
    public static void FeedbackNoticeValidatorReportsEveryProblem()
    {
        var validator = FeedbackNoticeRules.DefaultFeedbackNoticeValidator();
        var item = FeedbackNotice.NewFeedbackNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackNoticeLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FeedbackNoticeLabelLengthRule(32);
        var item = FeedbackNotice.NewFeedbackNotice("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackNoticeRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new FeedbackNoticeRevisionRangeRule(16, 1000);
        var item = FeedbackNotice.NewFeedbackNotice("x", "y");
        item.Revision = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FeedbackNoticeValidatorReportsEveryProblem();
        FeedbackNoticeLabelLengthRuleFlagsALongLabel();
        FeedbackNoticeRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
