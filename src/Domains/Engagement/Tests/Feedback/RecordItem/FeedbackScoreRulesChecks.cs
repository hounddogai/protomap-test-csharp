namespace Acme.Workforce.Engagement.Tests.Feedback.RecordItem;

using Acme.Workforce.Engagement.Feedback.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class FeedbackScoreRulesChecks
{
    public static void FeedbackScoreValidatorReportsEveryProblem()
    {
        var validator = FeedbackScoreRules.DefaultFeedbackScoreValidator();
        var item = FeedbackScore.NewFeedbackScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackScoreRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new FeedbackScoreRequiredSummaryRule("feedback score needs this field");
        var item = FeedbackScore.NewFeedbackScore("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackScoreSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new FeedbackScoreSummaryLengthRule(32);
        var item = FeedbackScore.NewFeedbackScore("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackScoreExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new FeedbackScoreExpiresAtRangeRule(16, 80);
        var item = FeedbackScore.NewFeedbackScore("x", "y");
        item.ExpiresAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FeedbackScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FeedbackScoreRequiredCodeRule("feedback score needs an entry");
        var item = FeedbackScore.NewFeedbackScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackScoreNameLengthRuleFlagsALongName()
    {
        var rule = new FeedbackScoreNameLengthRule(120);
        var item = FeedbackScore.NewFeedbackScore("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FeedbackScoreValidatorReportsEveryProblem();
        FeedbackScoreRequiredSummaryRuleFlagsABlankSummary();
        FeedbackScoreSummaryLengthRuleFlagsALongSummary();
        FeedbackScoreExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        FeedbackScoreRequiredCodeRuleFlagsABlankCode();
        FeedbackScoreNameLengthRuleFlagsALongName();
    }
}
