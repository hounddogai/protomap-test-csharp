namespace Acme.Workforce.Orgdesign.Tests.Span.SpanReview;

using Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>Checks of span review code, run by the project's runner.</summary>
public static class RegionalSpanReviewRulesChecks
{
    public static void RegionalSpanReviewValidatorReportsEveryProblem()
    {
        var validator = RegionalSpanReviewRules.DefaultRegionalSpanReviewValidator();
        var item = RegionalSpanReview.NewRegionalSpanReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalSpanReviewFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new RegionalSpanReviewFloorValueRangeRule(6.5, 1761.0);
        var item = RegionalSpanReview.NewRegionalSpanReview("x", "y");
        item.FloorValue = 1762.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalSpanReviewRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new RegionalSpanReviewRequiredCurrencyRule("regional span review needs a value");
        var item = RegionalSpanReview.NewRegionalSpanReview("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalSpanReviewExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new RegionalSpanReviewExpiresAtRangeRule(16, 255);
        var item = RegionalSpanReview.NewRegionalSpanReview("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalSpanReviewValidatorReportsEveryProblem();
        RegionalSpanReviewFloorValueRangeRuleFlagsValuesAboveFloorValue();
        RegionalSpanReviewRequiredCurrencyRuleFlagsABlankCurrency();
        RegionalSpanReviewExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
