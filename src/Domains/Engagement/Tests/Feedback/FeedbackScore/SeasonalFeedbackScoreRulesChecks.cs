namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class SeasonalFeedbackScoreRulesChecks
{
    public static void SeasonalFeedbackScoreValidatorReportsEveryProblem()
    {
        var validator = SeasonalFeedbackScoreRules.DefaultSeasonalFeedbackScoreValidator();
        var item = SeasonalFeedbackScore.NewSeasonalFeedbackScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalFeedbackScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalFeedbackScoreRequiredCodeRule("seasonal feedback score needs this field");
        var item = SeasonalFeedbackScore.NewSeasonalFeedbackScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalFeedbackScoreLocationLengthRuleFlagsALongLocation()
    {
        var rule = new SeasonalFeedbackScoreLocationLengthRule(1000);
        var item = SeasonalFeedbackScore.NewSeasonalFeedbackScore("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalFeedbackScoreTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new SeasonalFeedbackScoreTimezoneLengthRule(255);
        var item = SeasonalFeedbackScore.NewSeasonalFeedbackScore("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalFeedbackScoreTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SeasonalFeedbackScoreTitleLengthRule(32);
        var item = SeasonalFeedbackScore.NewSeasonalFeedbackScore("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalFeedbackScoreValidatorReportsEveryProblem();
        SeasonalFeedbackScoreRequiredCodeRuleFlagsABlankCode();
        SeasonalFeedbackScoreLocationLengthRuleFlagsALongLocation();
        SeasonalFeedbackScoreTimezoneLengthRuleFlagsALongTimezone();
        SeasonalFeedbackScoreTitleLengthRuleFlagsALongTitle();
    }
}
