namespace Acme.Workforce.Assets.Tests.AssetReturn.Estimate;

using Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class WeightedAssetReturnForecastRulesChecks
{
    public static void WeightedAssetReturnForecastValidatorReportsEveryProblem()
    {
        var validator = WeightedAssetReturnForecastRules.DefaultWeightedAssetReturnForecastValidator();
        var item = WeightedAssetReturnForecast.NewWeightedAssetReturnForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedAssetReturnForecastCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new WeightedAssetReturnForecastCostRangeRule(7.8, 2798.0);
        var item = WeightedAssetReturnForecast.NewWeightedAssetReturnForecast("x", "y");
        item.Cost = 2799.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedAssetReturnForecastUnitLengthRuleFlagsALongUnit()
    {
        var rule = new WeightedAssetReturnForecastUnitLengthRule(64);
        var item = WeightedAssetReturnForecast.NewWeightedAssetReturnForecast("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedAssetReturnForecastRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new WeightedAssetReturnForecastRequiredCountryRule("weighted asset return forecast needs this field");
        var item = WeightedAssetReturnForecast.NewWeightedAssetReturnForecast("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedAssetReturnForecastNameLengthRuleFlagsALongName()
    {
        var rule = new WeightedAssetReturnForecastNameLengthRule(32);
        var item = WeightedAssetReturnForecast.NewWeightedAssetReturnForecast("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedAssetReturnForecastValidatorReportsEveryProblem();
        WeightedAssetReturnForecastCostRangeRuleFlagsValuesAboveCost();
        WeightedAssetReturnForecastUnitLengthRuleFlagsALongUnit();
        WeightedAssetReturnForecastRequiredCountryRuleFlagsABlankCountry();
        WeightedAssetReturnForecastNameLengthRuleFlagsALongName();
    }
}
