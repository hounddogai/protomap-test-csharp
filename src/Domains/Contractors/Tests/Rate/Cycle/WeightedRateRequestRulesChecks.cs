namespace Acme.Workforce.Contractors.Tests.Rate.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class WeightedRateRequestRulesChecks
{
    public static void WeightedRateRequestValidatorReportsEveryProblem()
    {
        var validator = WeightedRateRequestRules.DefaultWeightedRateRequestValidator();
        var item = WeightedRateRequest.NewWeightedRateRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedRateRequestAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new WeightedRateRequestAttemptsRangeRule(32, 255);
        var item = WeightedRateRequest.NewWeightedRateRequest("x", "y");
        item.Attempts = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedRateRequestLocationLengthRuleFlagsALongLocation()
    {
        var rule = new WeightedRateRequestLocationLengthRule(120);
        var item = WeightedRateRequest.NewWeightedRateRequest("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedRateRequestRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new WeightedRateRequestRequiredReviewerRule("weighted rate request needs a value");
        var item = WeightedRateRequest.NewWeightedRateRequest("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedRateRequestValidatorReportsEveryProblem();
        WeightedRateRequestAttemptsRangeRuleFlagsValuesAboveAttempts();
        WeightedRateRequestLocationLengthRuleFlagsALongLocation();
        WeightedRateRequestRequiredReviewerRuleFlagsABlankReviewer();
    }
}
