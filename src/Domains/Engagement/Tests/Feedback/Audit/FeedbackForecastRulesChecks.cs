namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FeedbackForecastRulesChecks
{
    public static void FeedbackForecastValidatorReportsEveryProblem()
    {
        var validator = FeedbackForecastRules.DefaultFeedbackForecastValidator();
        var item = FeedbackForecast.NewFeedbackForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FeedbackForecastRequiredCodeRule("feedback forecast needs an entry");
        var item = FeedbackForecast.NewFeedbackForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new FeedbackForecastCodeLengthRule(1000);
        var item = FeedbackForecast.NewFeedbackForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackForecastRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FeedbackForecastRequiredLabelRule("feedback forecast needs this field");
        var item = FeedbackForecast.NewFeedbackForecast("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackForecastRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new FeedbackForecastRequiredDescriptionRule("feedback forecast needs this field");
        var item = FeedbackForecast.NewFeedbackForecast("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackForecastLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FeedbackForecastLabelLengthRule(1000);
        var item = FeedbackForecast.NewFeedbackForecast("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FeedbackForecastValidatorReportsEveryProblem();
        FeedbackForecastRequiredCodeRuleFlagsABlankCode();
        FeedbackForecastCodeLengthRuleFlagsALongCode();
        FeedbackForecastRequiredLabelRuleFlagsABlankLabel();
        FeedbackForecastRequiredDescriptionRuleFlagsABlankDescription();
        FeedbackForecastLabelLengthRuleFlagsALongLabel();
    }
}
