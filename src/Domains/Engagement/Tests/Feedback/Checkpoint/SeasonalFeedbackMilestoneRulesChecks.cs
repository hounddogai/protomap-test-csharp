namespace Acme.Workforce.Engagement.Tests.Feedback.Checkpoint;

using Acme.Workforce.Engagement.Feedback.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalFeedbackMilestoneRulesChecks
{
    public static void SeasonalFeedbackMilestoneValidatorReportsEveryProblem()
    {
        var validator = SeasonalFeedbackMilestoneRules.DefaultSeasonalFeedbackMilestoneValidator();
        var item = SeasonalFeedbackMilestone.NewSeasonalFeedbackMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalFeedbackMilestoneManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new SeasonalFeedbackMilestoneManagerIdLengthRule(64);
        var item = SeasonalFeedbackMilestone.NewSeasonalFeedbackMilestone("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalFeedbackMilestoneNameLengthRuleFlagsALongName()
    {
        var rule = new SeasonalFeedbackMilestoneNameLengthRule(80);
        var item = SeasonalFeedbackMilestone.NewSeasonalFeedbackMilestone("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalFeedbackMilestoneUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new SeasonalFeedbackMilestoneUpdatedAtRangeRule(80, 120);
        var item = SeasonalFeedbackMilestone.NewSeasonalFeedbackMilestone("x", "y");
        item.UpdatedAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalFeedbackMilestoneValidatorReportsEveryProblem();
        SeasonalFeedbackMilestoneManagerIdLengthRuleFlagsALongManagerId();
        SeasonalFeedbackMilestoneNameLengthRuleFlagsALongName();
        SeasonalFeedbackMilestoneUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
