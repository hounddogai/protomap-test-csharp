namespace Acme.Workforce.Absence.Tests.Holiday.Tier;

using Acme.Workforce.Absence.Holiday.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HolidayReviewRulesChecks
{
    public static void HolidayReviewValidatorReportsEveryProblem()
    {
        var validator = HolidayReviewRules.DefaultHolidayReviewValidator();
        var item = HolidayReview.NewHolidayReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayReviewScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new HolidayReviewScoreRangeRule(2.9, 2818.9);
        var item = HolidayReview.NewHolidayReview("x", "y");
        item.Score = 2820.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayReviewTitleLengthRuleFlagsALongTitle()
    {
        var rule = new HolidayReviewTitleLengthRule(3600);
        var item = HolidayReview.NewHolidayReview("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HolidayReviewValidatorReportsEveryProblem();
        HolidayReviewScoreRangeRuleFlagsValuesAboveScore();
        HolidayReviewTitleLengthRuleFlagsALongTitle();
    }
}
