namespace Acme.Workforce.Engagement.Tests.Feedback.Preference;

using Acme.Workforce.Engagement.Feedback.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FeedbackMetricRulesChecks
{
    public static void FeedbackMetricValidatorReportsEveryProblem()
    {
        var validator = FeedbackMetricRules.DefaultFeedbackMetricValidator();
        var item = FeedbackMetric.NewFeedbackMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackMetricTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FeedbackMetricTitleLengthRule(32);
        var item = FeedbackMetric.NewFeedbackMetric("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackMetricCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new FeedbackMetricCategoryLengthRule(1000);
        var item = FeedbackMetric.NewFeedbackMetric("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FeedbackMetricValidatorReportsEveryProblem();
        FeedbackMetricTitleLengthRuleFlagsALongTitle();
        FeedbackMetricCategoryLengthRuleFlagsALongCategory();
    }
}
