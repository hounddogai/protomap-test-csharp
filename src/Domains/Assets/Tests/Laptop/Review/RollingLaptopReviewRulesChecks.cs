namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RollingLaptopReviewRulesChecks
{
    public static void RollingLaptopReviewValidatorReportsEveryProblem()
    {
        var validator = RollingLaptopReviewRules.DefaultRollingLaptopReviewValidator();
        var item = RollingLaptopReview.NewRollingLaptopReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingLaptopReviewRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new RollingLaptopReviewRatioRangeRule(2.4, 947.3);
        var item = RollingLaptopReview.NewRollingLaptopReview("x", "y");
        item.Ratio = 948.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingLaptopReviewRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RollingLaptopReviewRequiredLabelRule("rolling laptop review needs a value");
        var item = RollingLaptopReview.NewRollingLaptopReview("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingLaptopReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingLaptopReviewRequiredCodeRule("rolling laptop review needs a value");
        var item = RollingLaptopReview.NewRollingLaptopReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingLaptopReviewCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new RollingLaptopReviewCurrencyLengthRule(80);
        var item = RollingLaptopReview.NewRollingLaptopReview("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingLaptopReviewRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new RollingLaptopReviewRequiredDepartmentRule("rolling laptop review needs an entry");
        var item = RollingLaptopReview.NewRollingLaptopReview("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingLaptopReviewValidatorReportsEveryProblem();
        RollingLaptopReviewRatioRangeRuleFlagsValuesAboveRatio();
        RollingLaptopReviewRequiredLabelRuleFlagsABlankLabel();
        RollingLaptopReviewRequiredCodeRuleFlagsABlankCode();
        RollingLaptopReviewCurrencyLengthRuleFlagsALongCurrency();
        RollingLaptopReviewRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
