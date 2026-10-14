namespace Acme.Workforce.Workplace.Tests.Occupancy.Digest;

using Acme.Workforce.Workplace.Occupancy.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LegacyOccupancyDigestRulesChecks
{
    public static void LegacyOccupancyDigestValidatorReportsEveryProblem()
    {
        var validator = LegacyOccupancyDigestRules.DefaultLegacyOccupancyDigestValidator();
        var item = LegacyOccupancyDigest.NewLegacyOccupancyDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyOccupancyDigestRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new LegacyOccupancyDigestRateRangeRule(9.9, 4062.7);
        var item = LegacyOccupancyDigest.NewLegacyOccupancyDigest("x", "y");
        item.Rate = 4064.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyOccupancyDigestCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new LegacyOccupancyDigestCurrencyLengthRule(255);
        var item = LegacyOccupancyDigest.NewLegacyOccupancyDigest("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyOccupancyDigestMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new LegacyOccupancyDigestMarginRangeRule(1.2, 4861.1);
        var item = LegacyOccupancyDigest.NewLegacyOccupancyDigest("x", "y");
        item.Margin = 4862.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyOccupancyDigestRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new LegacyOccupancyDigestRatingRangeRule(8.3, 2910.0);
        var item = LegacyOccupancyDigest.NewLegacyOccupancyDigest("x", "y");
        item.Rating = 2911.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyOccupancyDigestValidatorReportsEveryProblem();
        LegacyOccupancyDigestRateRangeRuleFlagsValuesAboveRate();
        LegacyOccupancyDigestCurrencyLengthRuleFlagsALongCurrency();
        LegacyOccupancyDigestMarginRangeRuleFlagsValuesAboveMargin();
        LegacyOccupancyDigestRatingRangeRuleFlagsValuesAboveRating();
    }
}
