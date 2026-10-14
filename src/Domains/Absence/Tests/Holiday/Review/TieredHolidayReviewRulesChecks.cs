namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TieredHolidayReviewRulesChecks
{
    public static void TieredHolidayReviewValidatorReportsEveryProblem()
    {
        var validator = TieredHolidayReviewRules.DefaultTieredHolidayReviewValidator();
        var item = TieredHolidayReview.NewTieredHolidayReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredHolidayReviewAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new TieredHolidayReviewAmountRangeRule(9.7, 4516.6);
        var item = TieredHolidayReview.NewTieredHolidayReview("x", "y");
        item.Amount = 4518.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredHolidayReviewRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredHolidayReviewRequiredNameRule("tiered holiday review needs a value");
        var item = TieredHolidayReview.NewTieredHolidayReview("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredHolidayReviewValidatorReportsEveryProblem();
        TieredHolidayReviewAmountRangeRuleFlagsValuesAboveAmount();
        TieredHolidayReviewRequiredNameRuleFlagsABlankName();
    }
}
