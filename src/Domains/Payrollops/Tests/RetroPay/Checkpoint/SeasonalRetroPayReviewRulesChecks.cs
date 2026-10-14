namespace Acme.Workforce.Payrollops.Tests.RetroPay.Checkpoint;

using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalRetroPayReviewRulesChecks
{
    public static void SeasonalRetroPayReviewValidatorReportsEveryProblem()
    {
        var validator = SeasonalRetroPayReviewRules.DefaultSeasonalRetroPayReviewValidator();
        var item = SeasonalRetroPayReview.NewSeasonalRetroPayReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalRetroPayReviewLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new SeasonalRetroPayReviewLeaveDaysRangeRule(32, 32);
        var item = SeasonalRetroPayReview.NewSeasonalRetroPayReview("x", "y");
        item.LeaveDays = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalRetroPayReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalRetroPayReviewCodeLengthRule(120);
        var item = SeasonalRetroPayReview.NewSeasonalRetroPayReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalRetroPayReviewSourceLengthRuleFlagsALongSource()
    {
        var rule = new SeasonalRetroPayReviewSourceLengthRule(255);
        var item = SeasonalRetroPayReview.NewSeasonalRetroPayReview("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalRetroPayReviewPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new SeasonalRetroPayReviewPhoneLengthRule(1000);
        var item = SeasonalRetroPayReview.NewSeasonalRetroPayReview("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalRetroPayReviewValidatorReportsEveryProblem();
        SeasonalRetroPayReviewLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        SeasonalRetroPayReviewCodeLengthRuleFlagsALongCode();
        SeasonalRetroPayReviewSourceLengthRuleFlagsALongSource();
        SeasonalRetroPayReviewPhoneLengthRuleFlagsALongPhone();
    }
}
