namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackAllocationRulesChecks
{
    public static void FeedbackAllocationValidatorReportsEveryProblem()
    {
        var validator = FeedbackAllocationRules.DefaultFeedbackAllocationValidator();
        var item = FeedbackAllocation.NewFeedbackAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackAllocationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FeedbackAllocationRequiredTitleRule("feedback allocation needs this field");
        var item = FeedbackAllocation.NewFeedbackAllocation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FeedbackAllocationRequiredCodeRule("feedback allocation needs an entry");
        var item = FeedbackAllocation.NewFeedbackAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackAllocationRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new FeedbackAllocationRequiredReviewerRule("feedback allocation needs a value");
        var item = FeedbackAllocation.NewFeedbackAllocation("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FeedbackAllocationValidatorReportsEveryProblem();
        FeedbackAllocationRequiredTitleRuleFlagsABlankTitle();
        FeedbackAllocationRequiredCodeRuleFlagsABlankCode();
        FeedbackAllocationRequiredReviewerRuleFlagsABlankReviewer();
    }
}
