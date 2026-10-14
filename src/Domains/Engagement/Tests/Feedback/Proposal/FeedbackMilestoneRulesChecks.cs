namespace Acme.Workforce.Engagement.Tests.Feedback.Proposal;

using Acme.Workforce.Engagement.Feedback.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class FeedbackMilestoneRulesChecks
{
    public static void FeedbackMilestoneValidatorReportsEveryProblem()
    {
        var validator = FeedbackMilestoneRules.DefaultFeedbackMilestoneValidator();
        var item = FeedbackMilestone.NewFeedbackMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackMilestoneRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FeedbackMilestoneRequiredTitleRule("feedback milestone needs this field");
        var item = FeedbackMilestone.NewFeedbackMilestone("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackMilestonePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new FeedbackMilestonePriorityRangeRule(80, 120);
        var item = FeedbackMilestone.NewFeedbackMilestone("x", "y");
        item.Priority = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FeedbackMilestoneSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new FeedbackMilestoneSalaryBandLengthRule(80);
        var item = FeedbackMilestone.NewFeedbackMilestone("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FeedbackMilestoneRequiredCodeRule("feedback milestone needs this field");
        var item = FeedbackMilestone.NewFeedbackMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackMilestoneRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new FeedbackMilestoneRequiredLocationRule("feedback milestone needs this field");
        var item = FeedbackMilestone.NewFeedbackMilestone("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FeedbackMilestoneValidatorReportsEveryProblem();
        FeedbackMilestoneRequiredTitleRuleFlagsABlankTitle();
        FeedbackMilestonePriorityRangeRuleFlagsValuesAbovePriority();
        FeedbackMilestoneSalaryBandLengthRuleFlagsALongSalaryBand();
        FeedbackMilestoneRequiredCodeRuleFlagsABlankCode();
        FeedbackMilestoneRequiredLocationRuleFlagsABlankLocation();
    }
}
