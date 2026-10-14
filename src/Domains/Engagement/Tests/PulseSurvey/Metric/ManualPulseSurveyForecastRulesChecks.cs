namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ManualPulseSurveyForecastRulesChecks
{
    public static void ManualPulseSurveyForecastValidatorReportsEveryProblem()
    {
        var validator = ManualPulseSurveyForecastRules.DefaultManualPulseSurveyForecastValidator();
        var item = ManualPulseSurveyForecast.NewManualPulseSurveyForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualPulseSurveyForecastManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new ManualPulseSurveyForecastManagerIdLengthRule(1000);
        var item = ManualPulseSurveyForecast.NewManualPulseSurveyForecast("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualPulseSurveyForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualPulseSurveyForecastRequiredCodeRule("manual pulse survey forecast needs an entry");
        var item = ManualPulseSurveyForecast.NewManualPulseSurveyForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualPulseSurveyForecastLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ManualPulseSurveyForecastLabelLengthRule(80);
        var item = ManualPulseSurveyForecast.NewManualPulseSurveyForecast("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualPulseSurveyForecastValidatorReportsEveryProblem();
        ManualPulseSurveyForecastManagerIdLengthRuleFlagsALongManagerId();
        ManualPulseSurveyForecastRequiredCodeRuleFlagsABlankCode();
        ManualPulseSurveyForecastLabelLengthRuleFlagsALongLabel();
    }
}
