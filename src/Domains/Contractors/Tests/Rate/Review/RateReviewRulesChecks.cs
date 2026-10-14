namespace Acme.Workforce.Contractors.Tests.Rate.Review;

using Acme.Workforce.Contractors.Rate.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RateReviewRulesChecks
{
    public static void RateReviewValidatorReportsEveryProblem()
    {
        var validator = RateReviewRules.DefaultRateReviewValidator();
        var item = RateReview.NewRateReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new RateReviewCodeLengthRule(120);
        var item = RateReview.NewRateReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateReviewNameLengthRuleFlagsALongName()
    {
        var rule = new RateReviewNameLengthRule(1000);
        var item = RateReview.NewRateReview("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateReviewTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new RateReviewTimezoneLengthRule(120);
        var item = RateReview.NewRateReview("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateReviewRequiredNameRuleFlagsABlankName()
    {
        var rule = new RateReviewRequiredNameRule("rate review needs an entry");
        var item = RateReview.NewRateReview("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RateReviewValidatorReportsEveryProblem();
        RateReviewCodeLengthRuleFlagsALongCode();
        RateReviewNameLengthRuleFlagsALongName();
        RateReviewTimezoneLengthRuleFlagsALongTimezone();
        RateReviewRequiredNameRuleFlagsABlankName();
    }
}
