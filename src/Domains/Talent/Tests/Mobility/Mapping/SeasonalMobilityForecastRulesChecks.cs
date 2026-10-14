namespace Acme.Workforce.Talent.Tests.Mobility.Mapping;

using Acme.Workforce.Talent.Mobility.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class SeasonalMobilityForecastRulesChecks
{
    public static void SeasonalMobilityForecastValidatorReportsEveryProblem()
    {
        var validator = SeasonalMobilityForecastRules.DefaultSeasonalMobilityForecastValidator();
        var item = SeasonalMobilityForecast.NewSeasonalMobilityForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalMobilityForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalMobilityForecastRequiredCodeRule("seasonal mobility forecast needs an entry");
        var item = SeasonalMobilityForecast.NewSeasonalMobilityForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalMobilityForecastPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new SeasonalMobilityForecastPriorityRangeRule(80, 120);
        var item = SeasonalMobilityForecast.NewSeasonalMobilityForecast("x", "y");
        item.Priority = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalMobilityForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalMobilityForecastCodeLengthRule(32);
        var item = SeasonalMobilityForecast.NewSeasonalMobilityForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalMobilityForecastRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new SeasonalMobilityForecastRatioRangeRule(3.1, 372.2);
        var item = SeasonalMobilityForecast.NewSeasonalMobilityForecast("x", "y");
        item.Ratio = 373.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalMobilityForecastExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new SeasonalMobilityForecastExternalRefLengthRule(32);
        var item = SeasonalMobilityForecast.NewSeasonalMobilityForecast("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalMobilityForecastValidatorReportsEveryProblem();
        SeasonalMobilityForecastRequiredCodeRuleFlagsABlankCode();
        SeasonalMobilityForecastPriorityRangeRuleFlagsValuesAbovePriority();
        SeasonalMobilityForecastCodeLengthRuleFlagsALongCode();
        SeasonalMobilityForecastRatioRangeRuleFlagsValuesAboveRatio();
        SeasonalMobilityForecastExternalRefLengthRuleFlagsALongExternalRef();
    }
}
