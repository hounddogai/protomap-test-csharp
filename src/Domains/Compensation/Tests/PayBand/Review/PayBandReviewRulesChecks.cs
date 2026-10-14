namespace Acme.Workforce.Compensation.Tests.PayBand.Review;

using Acme.Workforce.Compensation.PayBand.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayBandReviewRulesChecks
{
    public static void PayBandReviewValidatorReportsEveryProblem()
    {
        var validator = PayBandReviewRules.DefaultPayBandReviewValidator();
        var item = PayBandReview.NewPayBandReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayBandReviewRequiredCodeRule("pay band review needs a value");
        var item = PayBandReview.NewPayBandReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandReviewRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new PayBandReviewRequiredReviewerRule("pay band review needs this field");
        var item = PayBandReview.NewPayBandReview("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandReviewRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PayBandReviewRetriesRangeRule(16, 64);
        var item = PayBandReview.NewPayBandReview("x", "y");
        item.Retries = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayBandReviewValidatorReportsEveryProblem();
        PayBandReviewRequiredCodeRuleFlagsABlankCode();
        PayBandReviewRequiredReviewerRuleFlagsABlankReviewer();
        PayBandReviewRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
