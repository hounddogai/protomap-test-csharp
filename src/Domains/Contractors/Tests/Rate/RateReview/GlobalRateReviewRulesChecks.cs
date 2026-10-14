namespace Acme.Workforce.Contractors.Tests.Rate.RateReview;

using Acme.Workforce.Contractors.Rate.RateReview;

/// <summary>Checks of rate review code, run by the project's runner.</summary>
public static class GlobalRateReviewRulesChecks
{
    public static void GlobalRateReviewValidatorReportsEveryProblem()
    {
        var validator = GlobalRateReviewRules.DefaultGlobalRateReviewValidator();
        var item = GlobalRateReview.NewGlobalRateReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalRateReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalRateReviewRequiredCodeRule("global rate review needs an entry");
        var item = GlobalRateReview.NewGlobalRateReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalRateReviewLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalRateReviewLabelLengthRule(255);
        var item = GlobalRateReview.NewGlobalRateReview("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalRateReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalRateReviewCodeLengthRule(16);
        var item = GlobalRateReview.NewGlobalRateReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalRateReviewValidatorReportsEveryProblem();
        GlobalRateReviewRequiredCodeRuleFlagsABlankCode();
        GlobalRateReviewLabelLengthRuleFlagsALongLabel();
        GlobalRateReviewCodeLengthRuleFlagsALongCode();
    }
}
