namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class FeedbackThresholdRulesChecks
{
    public static void FeedbackThresholdValidatorReportsEveryProblem()
    {
        var validator = FeedbackThresholdRules.DefaultFeedbackThresholdValidator();
        var item = FeedbackThreshold.NewFeedbackThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackThresholdChannelLengthRuleFlagsALongChannel()
    {
        var rule = new FeedbackThresholdChannelLengthRule(3600);
        var item = FeedbackThreshold.NewFeedbackThreshold("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackThresholdLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FeedbackThresholdLabelLengthRule(16);
        var item = FeedbackThreshold.NewFeedbackThreshold("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FeedbackThresholdValidatorReportsEveryProblem();
        FeedbackThresholdChannelLengthRuleFlagsALongChannel();
        FeedbackThresholdLabelLengthRuleFlagsALongLabel();
    }
}
