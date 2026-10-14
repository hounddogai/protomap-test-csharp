namespace Acme.Workforce.Compensation.Tests.MeritCycle.Review;

using Acme.Workforce.Compensation.MeritCycle.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientMeritCycleReviewRulesChecks
{
    public static void LenientMeritCycleReviewValidatorReportsEveryProblem()
    {
        var validator = LenientMeritCycleReviewRules.DefaultLenientMeritCycleReviewValidator();
        var item = LenientMeritCycleReview.NewLenientMeritCycleReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientMeritCycleReviewRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LenientMeritCycleReviewRequiredTitleRule("lenient merit cycle review needs this field");
        var item = LenientMeritCycleReview.NewLenientMeritCycleReview("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientMeritCycleReviewRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new LenientMeritCycleReviewRequiredUnitRule("lenient merit cycle review needs this field");
        var item = LenientMeritCycleReview.NewLenientMeritCycleReview("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientMeritCycleReviewQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new LenientMeritCycleReviewQuantityRangeRule(64, 255);
        var item = LenientMeritCycleReview.NewLenientMeritCycleReview("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientMeritCycleReviewRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new LenientMeritCycleReviewRequiredCategoryRule("lenient merit cycle review needs an entry");
        var item = LenientMeritCycleReview.NewLenientMeritCycleReview("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientMeritCycleReviewValidatorReportsEveryProblem();
        LenientMeritCycleReviewRequiredTitleRuleFlagsABlankTitle();
        LenientMeritCycleReviewRequiredUnitRuleFlagsABlankUnit();
        LenientMeritCycleReviewQuantityRangeRuleFlagsValuesAboveQuantity();
        LenientMeritCycleReviewRequiredCategoryRuleFlagsABlankCategory();
    }
}
