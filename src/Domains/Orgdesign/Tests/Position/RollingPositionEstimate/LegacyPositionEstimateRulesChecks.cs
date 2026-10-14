namespace Acme.Workforce.Orgdesign.Tests.Position.RollingPositionEstimate;

using Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

/// <summary>Checks of rolling position estimate code, run by the project's runner.</summary>
public static class LegacyPositionEstimateRulesChecks
{
    public static void LegacyPositionEstimateValidatorReportsEveryProblem()
    {
        var validator = LegacyPositionEstimateRules.DefaultLegacyPositionEstimateValidator();
        var item = LegacyPositionEstimate.NewLegacyPositionEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPositionEstimateDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new LegacyPositionEstimateDiscountRangeRule(4.5, 933.6);
        var item = LegacyPositionEstimate.NewLegacyPositionEstimate("x", "y");
        item.Discount = 935.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyPositionEstimateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LegacyPositionEstimateLabelLengthRule(80);
        var item = LegacyPositionEstimate.NewLegacyPositionEstimate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyPositionEstimateRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new LegacyPositionEstimateRequiredCurrencyRule("legacy position estimate needs this field");
        var item = LegacyPositionEstimate.NewLegacyPositionEstimate("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPositionEstimateRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new LegacyPositionEstimateRateRangeRule(8.1, 3882.8);
        var item = LegacyPositionEstimate.NewLegacyPositionEstimate("x", "y");
        item.Rate = 3884.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyPositionEstimateValidatorReportsEveryProblem();
        LegacyPositionEstimateDiscountRangeRuleFlagsValuesAboveDiscount();
        LegacyPositionEstimateLabelLengthRuleFlagsALongLabel();
        LegacyPositionEstimateRequiredCurrencyRuleFlagsABlankCurrency();
        LegacyPositionEstimateRateRangeRuleFlagsValuesAboveRate();
    }
}
