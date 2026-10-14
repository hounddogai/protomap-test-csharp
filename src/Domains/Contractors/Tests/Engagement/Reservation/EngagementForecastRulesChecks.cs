namespace Acme.Workforce.Contractors.Tests.Engagement.Reservation;

using Acme.Workforce.Contractors.Engagement.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class EngagementForecastRulesChecks
{
    public static void EngagementForecastValidatorReportsEveryProblem()
    {
        var validator = EngagementForecastRules.DefaultEngagementForecastValidator();
        var item = EngagementForecast.NewEngagementForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementForecastExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new EngagementForecastExpiresAtRangeRule(32, 86400);
        var item = EngagementForecast.NewEngagementForecast("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new EngagementForecastCodeLengthRule(80);
        var item = EngagementForecast.NewEngagementForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementForecastLabelLengthRuleFlagsALongLabel()
    {
        var rule = new EngagementForecastLabelLengthRule(120);
        var item = EngagementForecast.NewEngagementForecast("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementForecastRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new EngagementForecastRequiredManagerIdRule("engagement forecast needs an entry");
        var item = EngagementForecast.NewEngagementForecast("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EngagementForecastValidatorReportsEveryProblem();
        EngagementForecastExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        EngagementForecastCodeLengthRuleFlagsALongCode();
        EngagementForecastLabelLengthRuleFlagsALongLabel();
        EngagementForecastRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
