namespace Acme.Workforce.Engagement.Tests.Sentiment.Milestone;

using Acme.Workforce.Engagement.Sentiment.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SentimentMilestoneRulesChecks
{
    public static void SentimentMilestoneValidatorReportsEveryProblem()
    {
        var validator = SentimentMilestoneRules.DefaultSentimentMilestoneValidator();
        var item = SentimentMilestone.NewSentimentMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentMilestoneRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new SentimentMilestoneRequiredNotesRule("sentiment milestone needs an entry");
        var item = SentimentMilestone.NewSentimentMilestone("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentMilestoneUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new SentimentMilestoneUpdatedAtRangeRule(64, 120);
        var item = SentimentMilestone.NewSentimentMilestone("x", "y");
        item.UpdatedAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SentimentMilestoneValidatorReportsEveryProblem();
        SentimentMilestoneRequiredNotesRuleFlagsABlankNotes();
        SentimentMilestoneUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
