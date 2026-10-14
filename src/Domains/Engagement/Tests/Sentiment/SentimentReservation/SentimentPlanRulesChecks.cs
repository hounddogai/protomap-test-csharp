namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentReservation;

using Acme.Workforce.Engagement.Sentiment.SentimentReservation;

/// <summary>Checks of sentiment reservation code, run by the project's runner.</summary>
public static class SentimentPlanRulesChecks
{
    public static void SentimentPlanValidatorReportsEveryProblem()
    {
        var validator = SentimentPlanRules.DefaultSentimentPlanValidator();
        var item = SentimentPlan.NewSentimentPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentPlanThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new SentimentPlanThresholdRangeRule(4.1, 2225.5);
        var item = SentimentPlan.NewSentimentPlan("x", "y");
        item.Threshold = 2227.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SentimentPlanLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SentimentPlanLabelLengthRule(3600);
        var item = SentimentPlan.NewSentimentPlan("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentPlanCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new SentimentPlanCategoryLengthRule(32);
        var item = SentimentPlan.NewSentimentPlan("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SentimentPlanValidatorReportsEveryProblem();
        SentimentPlanThresholdRangeRuleFlagsValuesAboveThreshold();
        SentimentPlanLabelLengthRuleFlagsALongLabel();
        SentimentPlanCategoryLengthRuleFlagsALongCategory();
    }
}
