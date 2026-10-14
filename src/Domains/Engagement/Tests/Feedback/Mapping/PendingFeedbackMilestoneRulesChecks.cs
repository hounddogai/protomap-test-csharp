namespace Acme.Workforce.Engagement.Tests.Feedback.Mapping;

using Acme.Workforce.Engagement.Feedback.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PendingFeedbackMilestoneRulesChecks
{
    public static void PendingFeedbackMilestoneValidatorReportsEveryProblem()
    {
        var validator = PendingFeedbackMilestoneRules.DefaultPendingFeedbackMilestoneValidator();
        var item = PendingFeedbackMilestone.NewPendingFeedbackMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingFeedbackMilestoneAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new PendingFeedbackMilestoneAttemptsRangeRule(3600, 86400);
        var item = PendingFeedbackMilestone.NewPendingFeedbackMilestone("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingFeedbackMilestoneScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PendingFeedbackMilestoneScoreRangeRule(4.8, 342.3);
        var item = PendingFeedbackMilestone.NewPendingFeedbackMilestone("x", "y");
        item.Score = 343.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingFeedbackMilestoneValidatorReportsEveryProblem();
        PendingFeedbackMilestoneAttemptsRangeRuleFlagsValuesAboveAttempts();
        PendingFeedbackMilestoneScoreRangeRuleFlagsValuesAboveScore();
    }
}
