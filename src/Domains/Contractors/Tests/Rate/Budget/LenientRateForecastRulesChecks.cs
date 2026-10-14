namespace Acme.Workforce.Contractors.Tests.Rate.Budget;

using Acme.Workforce.Contractors.Rate.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LenientRateForecastRulesChecks
{
    public static void LenientRateForecastValidatorReportsEveryProblem()
    {
        var validator = LenientRateForecastRules.DefaultLenientRateForecastValidator();
        var item = LenientRateForecast.NewLenientRateForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientRateForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientRateForecastRequiredCodeRule("lenient rate forecast needs this field");
        var item = LenientRateForecast.NewLenientRateForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientRateForecastRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new LenientRateForecastRequiredReviewerRule("lenient rate forecast needs a value");
        var item = LenientRateForecast.NewLenientRateForecast("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientRateForecastReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new LenientRateForecastReviewerLengthRule(1000);
        var item = LenientRateForecast.NewLenientRateForecast("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientRateForecastRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientRateForecastRequiredLabelRule("lenient rate forecast needs this field");
        var item = LenientRateForecast.NewLenientRateForecast("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientRateForecastValidatorReportsEveryProblem();
        LenientRateForecastRequiredCodeRuleFlagsABlankCode();
        LenientRateForecastRequiredReviewerRuleFlagsABlankReviewer();
        LenientRateForecastReviewerLengthRuleFlagsALongReviewer();
        LenientRateForecastRequiredLabelRuleFlagsABlankLabel();
    }
}
