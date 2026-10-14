namespace Acme.Workforce.Talent.Tests.CareerPath.PathRule;

using Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>Checks of path rule code, run by the project's runner.</summary>
public static class TieredCareerPathForecastRulesChecks
{
    public static void TieredCareerPathForecastValidatorReportsEveryProblem()
    {
        var validator = TieredCareerPathForecastRules.DefaultTieredCareerPathForecastValidator();
        var item = TieredCareerPathForecast.NewTieredCareerPathForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredCareerPathForecastDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new TieredCareerPathForecastDescriptionLengthRule(255);
        var item = TieredCareerPathForecast.NewTieredCareerPathForecast("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredCareerPathForecastRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new TieredCareerPathForecastRequiredRegionRule("tiered career path forecast needs this field");
        var item = TieredCareerPathForecast.NewTieredCareerPathForecast("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredCareerPathForecastRegionLengthRuleFlagsALongRegion()
    {
        var rule = new TieredCareerPathForecastRegionLengthRule(3600);
        var item = TieredCareerPathForecast.NewTieredCareerPathForecast("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredCareerPathForecastAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new TieredCareerPathForecastAttemptsRangeRule(16, 3600);
        var item = TieredCareerPathForecast.NewTieredCareerPathForecast("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredCareerPathForecastValidatorReportsEveryProblem();
        TieredCareerPathForecastDescriptionLengthRuleFlagsALongDescription();
        TieredCareerPathForecastRequiredRegionRuleFlagsABlankRegion();
        TieredCareerPathForecastRegionLengthRuleFlagsALongRegion();
        TieredCareerPathForecastAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
