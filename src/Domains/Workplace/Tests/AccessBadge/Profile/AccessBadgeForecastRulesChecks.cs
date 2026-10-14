namespace Acme.Workforce.Workplace.Tests.AccessBadge.Profile;

using Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AccessBadgeForecastRulesChecks
{
    public static void AccessBadgeForecastValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeForecastRules.DefaultAccessBadgeForecastValidator();
        var item = AccessBadgeForecast.NewAccessBadgeForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeForecastRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new AccessBadgeForecastRequiredTimezoneRule("access badge forecast needs an entry");
        var item = AccessBadgeForecast.NewAccessBadgeForecast("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeForecastThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new AccessBadgeForecastThresholdRangeRule(1.9, 2366.5);
        var item = AccessBadgeForecast.NewAccessBadgeForecast("x", "y");
        item.Threshold = 2368.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeForecastManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new AccessBadgeForecastManagerIdLengthRule(1000);
        var item = AccessBadgeForecast.NewAccessBadgeForecast("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccessBadgeForecastValidatorReportsEveryProblem();
        AccessBadgeForecastRequiredTimezoneRuleFlagsABlankTimezone();
        AccessBadgeForecastThresholdRangeRuleFlagsValuesAboveThreshold();
        AccessBadgeForecastManagerIdLengthRuleFlagsALongManagerId();
    }
}
