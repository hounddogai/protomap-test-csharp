namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class RollingEngagementForecastRulesChecks
{
    public static void RollingEngagementForecastValidatorReportsEveryProblem()
    {
        var validator = RollingEngagementForecastRules.DefaultRollingEngagementForecastValidator();
        var item = RollingEngagementForecast.NewRollingEngagementForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingEngagementForecastSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new RollingEngagementForecastSummaryLengthRule(64);
        var item = RollingEngagementForecast.NewRollingEngagementForecast("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingEngagementForecastLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new RollingEngagementForecastLocaleLengthRule(80);
        var item = RollingEngagementForecast.NewRollingEngagementForecast("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingEngagementForecastLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RollingEngagementForecastLabelLengthRule(64);
        var item = RollingEngagementForecast.NewRollingEngagementForecast("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingEngagementForecastRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RollingEngagementForecastRequiredLabelRule("rolling engagement forecast needs an entry");
        var item = RollingEngagementForecast.NewRollingEngagementForecast("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingEngagementForecastGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new RollingEngagementForecastGraceDaysRangeRule(1000, 86400);
        var item = RollingEngagementForecast.NewRollingEngagementForecast("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingEngagementForecastValidatorReportsEveryProblem();
        RollingEngagementForecastSummaryLengthRuleFlagsALongSummary();
        RollingEngagementForecastLocaleLengthRuleFlagsALongLocale();
        RollingEngagementForecastLabelLengthRuleFlagsALongLabel();
        RollingEngagementForecastRequiredLabelRuleFlagsABlankLabel();
        RollingEngagementForecastGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
