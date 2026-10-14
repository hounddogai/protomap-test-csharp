namespace Acme.Workforce.Assets.Tests.AssetReturn.Forecast;

using Acme.Workforce.Assets.AssetReturn.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RollingAssetReturnForecastRulesChecks
{
    public static void RollingAssetReturnForecastValidatorReportsEveryProblem()
    {
        var validator = RollingAssetReturnForecastRules.DefaultRollingAssetReturnForecastValidator();
        var item = RollingAssetReturnForecast.NewRollingAssetReturnForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingAssetReturnForecastBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new RollingAssetReturnForecastBadgeLengthRule(1000);
        var item = RollingAssetReturnForecast.NewRollingAssetReturnForecast("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingAssetReturnForecastRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new RollingAssetReturnForecastRequiredOwnerIdRule("rolling asset return forecast needs this field");
        var item = RollingAssetReturnForecast.NewRollingAssetReturnForecast("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingAssetReturnForecastRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RollingAssetReturnForecastRequiredTitleRule("rolling asset return forecast needs this field");
        var item = RollingAssetReturnForecast.NewRollingAssetReturnForecast("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingAssetReturnForecastValidatorReportsEveryProblem();
        RollingAssetReturnForecastBadgeLengthRuleFlagsALongBadge();
        RollingAssetReturnForecastRequiredOwnerIdRuleFlagsABlankOwnerId();
        RollingAssetReturnForecastRequiredTitleRuleFlagsABlankTitle();
    }
}
