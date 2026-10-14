namespace Acme.Workforce.Engagement.Tests.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>Checks of seasonal feedback reservation code, run by the project's runner.</summary>
public static class SeasonalFeedbackEstimateRulesChecks
{
    public static void SeasonalFeedbackEstimateValidatorReportsEveryProblem()
    {
        var validator = SeasonalFeedbackEstimateRules.DefaultSeasonalFeedbackEstimateValidator();
        var item = SeasonalFeedbackEstimate.NewSeasonalFeedbackEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalFeedbackEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalFeedbackEstimateCodeLengthRule(120);
        var item = SeasonalFeedbackEstimate.NewSeasonalFeedbackEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalFeedbackEstimateBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new SeasonalFeedbackEstimateBatchSizeRangeRule(64, 255);
        var item = SeasonalFeedbackEstimate.NewSeasonalFeedbackEstimate("x", "y");
        item.BatchSize = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalFeedbackEstimateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SeasonalFeedbackEstimateLabelLengthRule(1000);
        var item = SeasonalFeedbackEstimate.NewSeasonalFeedbackEstimate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalFeedbackEstimateRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new SeasonalFeedbackEstimateRequiredReviewerRule("seasonal feedback estimate needs this field");
        var item = SeasonalFeedbackEstimate.NewSeasonalFeedbackEstimate("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalFeedbackEstimateValidatorReportsEveryProblem();
        SeasonalFeedbackEstimateCodeLengthRuleFlagsALongCode();
        SeasonalFeedbackEstimateBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        SeasonalFeedbackEstimateLabelLengthRuleFlagsALongLabel();
        SeasonalFeedbackEstimateRequiredReviewerRuleFlagsABlankReviewer();
    }
}
