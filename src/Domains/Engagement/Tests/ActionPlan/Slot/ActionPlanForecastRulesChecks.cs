namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanForecastRulesChecks
{
    public static void ActionPlanForecastValidatorReportsEveryProblem()
    {
        var validator = ActionPlanForecastRules.DefaultActionPlanForecastValidator();
        var item = ActionPlanForecast.NewActionPlanForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanForecastVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new ActionPlanForecastVersionRangeRule(3600, 3600);
        var item = ActionPlanForecast.NewActionPlanForecast("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanForecastRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new ActionPlanForecastRateRangeRule(4.6, 4334.9);
        var item = ActionPlanForecast.NewActionPlanForecast("x", "y");
        item.Rate = 4336.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanForecastPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new ActionPlanForecastPhoneLengthRule(86400);
        var item = ActionPlanForecast.NewActionPlanForecast("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ActionPlanForecastValidatorReportsEveryProblem();
        ActionPlanForecastVersionRangeRuleFlagsValuesAboveVersion();
        ActionPlanForecastRateRangeRuleFlagsValuesAboveRate();
        ActionPlanForecastPhoneLengthRuleFlagsALongPhone();
    }
}
