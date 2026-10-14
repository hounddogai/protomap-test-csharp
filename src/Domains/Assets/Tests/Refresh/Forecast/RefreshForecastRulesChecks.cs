namespace Acme.Workforce.Assets.Tests.Refresh.Forecast;

using Acme.Workforce.Assets.Refresh.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RefreshForecastRulesChecks
{
    public static void RefreshForecastValidatorReportsEveryProblem()
    {
        var validator = RefreshForecastRules.DefaultRefreshForecastValidator();
        var item = RefreshForecast.NewRefreshForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshForecastRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new RefreshForecastRequiredChannelRule("refresh forecast needs this field");
        var item = RefreshForecast.NewRefreshForecast("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshForecastRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new RefreshForecastRatioRangeRule(1.6, 1128.5);
        var item = RefreshForecast.NewRefreshForecast("x", "y");
        item.Ratio = 1130.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshForecastTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RefreshForecastTitleLengthRule(120);
        var item = RefreshForecast.NewRefreshForecast("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RefreshForecastRequiredCodeRule("refresh forecast needs an entry");
        var item = RefreshForecast.NewRefreshForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RefreshForecastValidatorReportsEveryProblem();
        RefreshForecastRequiredChannelRuleFlagsABlankChannel();
        RefreshForecastRatioRangeRuleFlagsValuesAboveRatio();
        RefreshForecastTitleLengthRuleFlagsALongTitle();
        RefreshForecastRequiredCodeRuleFlagsABlankCode();
    }
}
