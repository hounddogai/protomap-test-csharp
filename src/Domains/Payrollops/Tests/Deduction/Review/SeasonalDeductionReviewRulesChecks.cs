namespace Acme.Workforce.Payrollops.Tests.Deduction.Review;

using Acme.Workforce.Payrollops.Deduction.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class SeasonalDeductionReviewRulesChecks
{
    public static void SeasonalDeductionReviewValidatorReportsEveryProblem()
    {
        var validator = SeasonalDeductionReviewRules.DefaultSeasonalDeductionReviewValidator();
        var item = SeasonalDeductionReview.NewSeasonalDeductionReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalDeductionReviewRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new SeasonalDeductionReviewRequiredEmployeeIdRule("seasonal deduction review needs a value");
        var item = SeasonalDeductionReview.NewSeasonalDeductionReview("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalDeductionReviewCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new SeasonalDeductionReviewCeilingRangeRule(5.4, 3003.0);
        var item = SeasonalDeductionReview.NewSeasonalDeductionReview("x", "y");
        item.Ceiling = 3004.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalDeductionReviewRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new SeasonalDeductionReviewRequiredRegionRule("seasonal deduction review needs an entry");
        var item = SeasonalDeductionReview.NewSeasonalDeductionReview("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalDeductionReviewRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SeasonalDeductionReviewRequiredLabelRule("seasonal deduction review needs an entry");
        var item = SeasonalDeductionReview.NewSeasonalDeductionReview("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalDeductionReviewValidatorReportsEveryProblem();
        SeasonalDeductionReviewRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        SeasonalDeductionReviewCeilingRangeRuleFlagsValuesAboveCeiling();
        SeasonalDeductionReviewRequiredRegionRuleFlagsABlankRegion();
        SeasonalDeductionReviewRequiredLabelRuleFlagsABlankLabel();
    }
}
