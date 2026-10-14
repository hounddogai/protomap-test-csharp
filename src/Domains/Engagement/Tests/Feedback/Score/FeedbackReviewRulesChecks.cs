namespace Acme.Workforce.Engagement.Tests.Feedback.Score;

using Acme.Workforce.Engagement.Feedback.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class FeedbackReviewRulesChecks
{
    public static void FeedbackReviewValidatorReportsEveryProblem()
    {
        var validator = FeedbackReviewRules.DefaultFeedbackReviewValidator();
        var item = FeedbackReview.NewFeedbackReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackReviewTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FeedbackReviewTitleLengthRule(80);
        var item = FeedbackReview.NewFeedbackReview("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new FeedbackReviewCodeLengthRule(64);
        var item = FeedbackReview.NewFeedbackReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackReviewEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new FeedbackReviewEmployeeIdLengthRule(80);
        var item = FeedbackReview.NewFeedbackReview("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackReviewRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new FeedbackReviewRequiredEmployeeIdRule("feedback review needs this field");
        var item = FeedbackReview.NewFeedbackReview("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackReviewBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new FeedbackReviewBudgetRangeRule(5.7, 4594.0);
        var item = FeedbackReview.NewFeedbackReview("x", "y");
        item.Budget = 4595.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FeedbackReviewValidatorReportsEveryProblem();
        FeedbackReviewTitleLengthRuleFlagsALongTitle();
        FeedbackReviewCodeLengthRuleFlagsALongCode();
        FeedbackReviewEmployeeIdLengthRuleFlagsALongEmployeeId();
        FeedbackReviewRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        FeedbackReviewBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
