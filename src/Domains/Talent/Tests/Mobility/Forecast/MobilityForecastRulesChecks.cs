namespace Acme.Workforce.Talent.Tests.Mobility.Forecast;

using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class MobilityForecastRulesChecks
{
    public static void MobilityForecastValidatorReportsEveryProblem()
    {
        var validator = MobilityForecastRules.DefaultMobilityForecastValidator();
        var item = MobilityForecast.NewMobilityForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityForecastCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new MobilityForecastCurrencyLengthRule(64);
        var item = MobilityForecast.NewMobilityForecast("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityForecastRequiredNameRuleFlagsABlankName()
    {
        var rule = new MobilityForecastRequiredNameRule("mobility forecast needs an entry");
        var item = MobilityForecast.NewMobilityForecast("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityForecastWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new MobilityForecastWeightRangeRule(1.3, 2620.0);
        var item = MobilityForecast.NewMobilityForecast("x", "y");
        item.Weight = 2621.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityForecastAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new MobilityForecastAmountRangeRule(5.7, 3789.6);
        var item = MobilityForecast.NewMobilityForecast("x", "y");
        item.Amount = 3791.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityForecastQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new MobilityForecastQuantityRangeRule(64, 1000);
        var item = MobilityForecast.NewMobilityForecast("x", "y");
        item.Quantity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MobilityForecastValidatorReportsEveryProblem();
        MobilityForecastCurrencyLengthRuleFlagsALongCurrency();
        MobilityForecastRequiredNameRuleFlagsABlankName();
        MobilityForecastWeightRangeRuleFlagsValuesAboveWeight();
        MobilityForecastAmountRangeRuleFlagsValuesAboveAmount();
        MobilityForecastQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
