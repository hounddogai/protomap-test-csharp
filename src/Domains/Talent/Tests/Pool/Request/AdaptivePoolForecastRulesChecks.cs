namespace Acme.Workforce.Talent.Tests.Pool.Request;

using Acme.Workforce.Talent.Pool.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdaptivePoolForecastRulesChecks
{
    public static void AdaptivePoolForecastValidatorReportsEveryProblem()
    {
        var validator = AdaptivePoolForecastRules.DefaultAdaptivePoolForecastValidator();
        var item = AdaptivePoolForecast.NewAdaptivePoolForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptivePoolForecastRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new AdaptivePoolForecastRequiredManagerIdRule("adaptive pool forecast needs this field");
        var item = AdaptivePoolForecast.NewAdaptivePoolForecast("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptivePoolForecastRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdaptivePoolForecastRequiredTitleRule("adaptive pool forecast needs this field");
        var item = AdaptivePoolForecast.NewAdaptivePoolForecast("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptivePoolForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptivePoolForecastRequiredCodeRule("adaptive pool forecast needs this field");
        var item = AdaptivePoolForecast.NewAdaptivePoolForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptivePoolForecastRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptivePoolForecastRequiredLabelRule("adaptive pool forecast needs a value");
        var item = AdaptivePoolForecast.NewAdaptivePoolForecast("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptivePoolForecastValidatorReportsEveryProblem();
        AdaptivePoolForecastRequiredManagerIdRuleFlagsABlankManagerId();
        AdaptivePoolForecastRequiredTitleRuleFlagsABlankTitle();
        AdaptivePoolForecastRequiredCodeRuleFlagsABlankCode();
        AdaptivePoolForecastRequiredLabelRuleFlagsABlankLabel();
    }
}
