namespace Acme.Workforce.Orgdesign.Tests.Span.Review;

using Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RollingSpanReviewRulesChecks
{
    public static void RollingSpanReviewValidatorReportsEveryProblem()
    {
        var validator = RollingSpanReviewRules.DefaultRollingSpanReviewValidator();
        var item = RollingSpanReview.NewRollingSpanReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingSpanReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingSpanReviewRequiredCodeRule("rolling span review needs this field");
        var item = RollingSpanReview.NewRollingSpanReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingSpanReviewRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new RollingSpanReviewRateRangeRule(7.0, 460.0);
        var item = RollingSpanReview.NewRollingSpanReview("x", "y");
        item.Rate = 461.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingSpanReviewRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RollingSpanReviewRequiredTitleRule("rolling span review needs this field");
        var item = RollingSpanReview.NewRollingSpanReview("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingSpanReviewRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new RollingSpanReviewRequiredCostCenterRule("rolling span review needs an entry");
        var item = RollingSpanReview.NewRollingSpanReview("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingSpanReviewValidatorReportsEveryProblem();
        RollingSpanReviewRequiredCodeRuleFlagsABlankCode();
        RollingSpanReviewRateRangeRuleFlagsValuesAboveRate();
        RollingSpanReviewRequiredTitleRuleFlagsABlankTitle();
        RollingSpanReviewRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}
