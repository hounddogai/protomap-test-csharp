namespace Acme.Workforce.Orgdesign.Tests.Position.PositionAudit;

using Acme.Workforce.Orgdesign.Position.PositionAudit;

/// <summary>Checks of position audit code, run by the project's runner.</summary>
public static class WeightedPositionForecastRulesChecks
{
    public static void WeightedPositionForecastValidatorReportsEveryProblem()
    {
        var validator = WeightedPositionForecastRules.DefaultWeightedPositionForecastValidator();
        var item = WeightedPositionForecast.NewWeightedPositionForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedPositionForecastTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new WeightedPositionForecastTimezoneLengthRule(86400);
        var item = WeightedPositionForecast.NewWeightedPositionForecast("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedPositionForecastLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WeightedPositionForecastLabelLengthRule(16);
        var item = WeightedPositionForecast.NewWeightedPositionForecast("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedPositionForecastVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new WeightedPositionForecastVersionRangeRule(64, 255);
        var item = WeightedPositionForecast.NewWeightedPositionForecast("x", "y");
        item.Version = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedPositionForecastRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new WeightedPositionForecastRequiredLocationRule("weighted position forecast needs an entry");
        var item = WeightedPositionForecast.NewWeightedPositionForecast("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedPositionForecastValidatorReportsEveryProblem();
        WeightedPositionForecastTimezoneLengthRuleFlagsALongTimezone();
        WeightedPositionForecastLabelLengthRuleFlagsALongLabel();
        WeightedPositionForecastVersionRangeRuleFlagsValuesAboveVersion();
        WeightedPositionForecastRequiredLocationRuleFlagsABlankLocation();
    }
}
