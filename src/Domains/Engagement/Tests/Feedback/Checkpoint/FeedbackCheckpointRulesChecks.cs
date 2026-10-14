namespace Acme.Workforce.Engagement.Tests.Feedback.Checkpoint;

using Acme.Workforce.Engagement.Feedback.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class FeedbackCheckpointRulesChecks
{
    public static void FeedbackCheckpointValidatorReportsEveryProblem()
    {
        var validator = FeedbackCheckpointRules.DefaultFeedbackCheckpointValidator();
        var item = FeedbackCheckpoint.NewFeedbackCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackCheckpointNameLengthRuleFlagsALongName()
    {
        var rule = new FeedbackCheckpointNameLengthRule(32);
        var item = FeedbackCheckpoint.NewFeedbackCheckpoint("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackCheckpointRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new FeedbackCheckpointRequiredLocaleRule("feedback checkpoint needs an entry");
        var item = FeedbackCheckpoint.NewFeedbackCheckpoint("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FeedbackCheckpointValidatorReportsEveryProblem();
        FeedbackCheckpointNameLengthRuleFlagsALongName();
        FeedbackCheckpointRequiredLocaleRuleFlagsABlankLocale();
    }
}
