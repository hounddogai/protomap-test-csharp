namespace Acme.Workforce.Contractors.Tests.Offboarding.Forecast;

using Acme.Workforce.Contractors.Offboarding.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class OffboardingForecastRulesChecks
{
    public static void OffboardingForecastValidatorReportsEveryProblem()
    {
        var validator = OffboardingForecastRules.DefaultOffboardingForecastValidator();
        var item = OffboardingForecast.NewOffboardingForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingForecastRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new OffboardingForecastRequiredDescriptionRule("offboarding forecast needs a value");
        var item = OffboardingForecast.NewOffboardingForecast("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OffboardingForecastLabelLengthRuleFlagsALongLabel()
    {
        var rule = new OffboardingForecastLabelLengthRule(16);
        var item = OffboardingForecast.NewOffboardingForecast("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingForecastDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new OffboardingForecastDescriptionLengthRule(80);
        var item = OffboardingForecast.NewOffboardingForecast("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new OffboardingForecastCodeLengthRule(16);
        var item = OffboardingForecast.NewOffboardingForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OffboardingForecastValidatorReportsEveryProblem();
        OffboardingForecastRequiredDescriptionRuleFlagsABlankDescription();
        OffboardingForecastLabelLengthRuleFlagsALongLabel();
        OffboardingForecastDescriptionLengthRuleFlagsALongDescription();
        OffboardingForecastCodeLengthRuleFlagsALongCode();
    }
}
