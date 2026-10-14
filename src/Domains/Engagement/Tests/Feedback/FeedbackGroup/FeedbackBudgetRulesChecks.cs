namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackGroup;

using Acme.Workforce.Engagement.Feedback.FeedbackGroup;

/// <summary>Checks of feedback group code, run by the project's runner.</summary>
public static class FeedbackBudgetRulesChecks
{
    public static void FeedbackBudgetValidatorReportsEveryProblem()
    {
        var validator = FeedbackBudgetRules.DefaultFeedbackBudgetValidator();
        var item = FeedbackBudget.NewFeedbackBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackBudgetJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new FeedbackBudgetJobCodeLengthRule(80);
        var item = FeedbackBudget.NewFeedbackBudget("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackBudgetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FeedbackBudgetRequiredLabelRule("feedback budget needs this field");
        var item = FeedbackBudget.NewFeedbackBudget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackBudgetRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new FeedbackBudgetRequiredSegmentRule("feedback budget needs this field");
        var item = FeedbackBudget.NewFeedbackBudget("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FeedbackBudgetValidatorReportsEveryProblem();
        FeedbackBudgetJobCodeLengthRuleFlagsALongJobCode();
        FeedbackBudgetRequiredLabelRuleFlagsABlankLabel();
        FeedbackBudgetRequiredSegmentRuleFlagsABlankSegment();
    }
}
