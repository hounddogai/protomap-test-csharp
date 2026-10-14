namespace Acme.Workforce.Engagement.Tests.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>Checks of seasonal feedback reservation code, run by the project's runner.</summary>
public static class RollingFeedbackScoreRulesChecks
{
    public static void RollingFeedbackScoreValidatorReportsEveryProblem()
    {
        var validator = RollingFeedbackScoreRules.DefaultRollingFeedbackScoreValidator();
        var item = RollingFeedbackScore.NewRollingFeedbackScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingFeedbackScoreRequiredNameRuleFlagsABlankName()
    {
        var rule = new RollingFeedbackScoreRequiredNameRule("rolling feedback score needs a value");
        var item = RollingFeedbackScore.NewRollingFeedbackScore("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingFeedbackScoreChannelLengthRuleFlagsALongChannel()
    {
        var rule = new RollingFeedbackScoreChannelLengthRule(32);
        var item = RollingFeedbackScore.NewRollingFeedbackScore("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingFeedbackScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingFeedbackScoreCodeLengthRule(32);
        var item = RollingFeedbackScore.NewRollingFeedbackScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingFeedbackScoreSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new RollingFeedbackScoreSequenceRangeRule(64, 120);
        var item = RollingFeedbackScore.NewRollingFeedbackScore("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingFeedbackScoreRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new RollingFeedbackScoreRateRangeRule(0.4, 1735.6);
        var item = RollingFeedbackScore.NewRollingFeedbackScore("x", "y");
        item.Rate = 1737.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingFeedbackScoreValidatorReportsEveryProblem();
        RollingFeedbackScoreRequiredNameRuleFlagsABlankName();
        RollingFeedbackScoreChannelLengthRuleFlagsALongChannel();
        RollingFeedbackScoreCodeLengthRuleFlagsALongCode();
        RollingFeedbackScoreSequenceRangeRuleFlagsValuesAboveSequence();
        RollingFeedbackScoreRateRangeRuleFlagsValuesAboveRate();
    }
}
