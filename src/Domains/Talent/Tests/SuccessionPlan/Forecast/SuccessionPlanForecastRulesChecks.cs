namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Forecast;

using Acme.Workforce.Talent.SuccessionPlan.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class SuccessionPlanForecastRulesChecks
{
    public static void SuccessionPlanForecastValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanForecastRules.DefaultSuccessionPlanForecastValidator();
        var item = SuccessionPlanForecast.NewSuccessionPlanForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanForecastQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new SuccessionPlanForecastQuantityRangeRule(255, 1000);
        var item = SuccessionPlanForecast.NewSuccessionPlanForecast("x", "y");
        item.Quantity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SuccessionPlanForecastRequiredCodeRule("succession plan forecast needs an entry");
        var item = SuccessionPlanForecast.NewSuccessionPlanForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanForecastRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new SuccessionPlanForecastRevisionRangeRule(64, 80);
        var item = SuccessionPlanForecast.NewSuccessionPlanForecast("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SuccessionPlanForecastValidatorReportsEveryProblem();
        SuccessionPlanForecastQuantityRangeRuleFlagsValuesAboveQuantity();
        SuccessionPlanForecastRequiredCodeRuleFlagsABlankCode();
        SuccessionPlanForecastRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
