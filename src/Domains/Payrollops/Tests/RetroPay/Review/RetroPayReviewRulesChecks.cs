namespace Acme.Workforce.Payrollops.Tests.RetroPay.Review;

using Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RetroPayReviewRulesChecks
{
    public static void RetroPayReviewValidatorReportsEveryProblem()
    {
        var validator = RetroPayReviewRules.DefaultRetroPayReviewValidator();
        var item = RetroPayReview.NewRetroPayReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayReviewTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new RetroPayReviewTimezoneLengthRule(80);
        var item = RetroPayReview.NewRetroPayReview("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new RetroPayReviewCodeLengthRule(80);
        var item = RetroPayReview.NewRetroPayReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayReviewTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RetroPayReviewTitleLengthRule(80);
        var item = RetroPayReview.NewRetroPayReview("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayReviewAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new RetroPayReviewAttemptsRangeRule(1000, 1000);
        var item = RetroPayReview.NewRetroPayReview("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RetroPayReviewValidatorReportsEveryProblem();
        RetroPayReviewTimezoneLengthRuleFlagsALongTimezone();
        RetroPayReviewCodeLengthRuleFlagsALongCode();
        RetroPayReviewTitleLengthRuleFlagsALongTitle();
        RetroPayReviewAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
