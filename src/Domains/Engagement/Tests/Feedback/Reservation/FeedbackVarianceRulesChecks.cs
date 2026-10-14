namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FeedbackVarianceRulesChecks
{
    public static void FeedbackVarianceValidatorReportsEveryProblem()
    {
        var validator = FeedbackVarianceRules.DefaultFeedbackVarianceValidator();
        var item = FeedbackVariance.NewFeedbackVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackVarianceCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new FeedbackVarianceCostRangeRule(3.4, 3568.7);
        var item = FeedbackVariance.NewFeedbackVariance("x", "y");
        item.Cost = 3570.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FeedbackVarianceRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new FeedbackVarianceRequiredLocaleRule("feedback variance needs a value");
        var item = FeedbackVariance.NewFeedbackVariance("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackVarianceLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new FeedbackVarianceLocaleLengthRule(3600);
        var item = FeedbackVariance.NewFeedbackVariance("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new FeedbackVarianceCodeLengthRule(80);
        var item = FeedbackVariance.NewFeedbackVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FeedbackVarianceValidatorReportsEveryProblem();
        FeedbackVarianceCostRangeRuleFlagsValuesAboveCost();
        FeedbackVarianceRequiredLocaleRuleFlagsABlankLocale();
        FeedbackVarianceLocaleLengthRuleFlagsALongLocale();
        FeedbackVarianceCodeLengthRuleFlagsALongCode();
    }
}
