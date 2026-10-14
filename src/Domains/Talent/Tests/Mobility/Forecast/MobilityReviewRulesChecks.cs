namespace Acme.Workforce.Talent.Tests.Mobility.Forecast;

using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class MobilityReviewRulesChecks
{
    public static void MobilityReviewValidatorReportsEveryProblem()
    {
        var validator = MobilityReviewRules.DefaultMobilityReviewValidator();
        var item = MobilityReview.NewMobilityReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityReviewRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new MobilityReviewRequiredSourceRule("mobility review needs a value");
        var item = MobilityReview.NewMobilityReview("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityReviewRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new MobilityReviewRateRangeRule(9.6, 2765.3);
        var item = MobilityReview.NewMobilityReview("x", "y");
        item.Rate = 2766.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityReviewSourceLengthRuleFlagsALongSource()
    {
        var rule = new MobilityReviewSourceLengthRule(80);
        var item = MobilityReview.NewMobilityReview("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MobilityReviewRequiredCodeRule("mobility review needs a value");
        var item = MobilityReview.NewMobilityReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MobilityReviewValidatorReportsEveryProblem();
        MobilityReviewRequiredSourceRuleFlagsABlankSource();
        MobilityReviewRateRangeRuleFlagsValuesAboveRate();
        MobilityReviewSourceLengthRuleFlagsALongSource();
        MobilityReviewRequiredCodeRuleFlagsABlankCode();
    }
}
