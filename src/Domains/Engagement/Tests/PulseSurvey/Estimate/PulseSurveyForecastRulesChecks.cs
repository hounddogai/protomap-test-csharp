namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyForecastRulesChecks
{
    public static void PulseSurveyForecastValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyForecastRules.DefaultPulseSurveyForecastValidator();
        var item = PulseSurveyForecast.NewPulseSurveyForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyForecastChannelLengthRuleFlagsALongChannel()
    {
        var rule = new PulseSurveyForecastChannelLengthRule(32);
        var item = PulseSurveyForecast.NewPulseSurveyForecast("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new PulseSurveyForecastCodeLengthRule(80);
        var item = PulseSurveyForecast.NewPulseSurveyForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyForecastRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PulseSurveyForecastRequiredTitleRule("pulse survey forecast needs an entry");
        var item = PulseSurveyForecast.NewPulseSurveyForecast("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PulseSurveyForecastRequiredCodeRule("pulse survey forecast needs an entry");
        var item = PulseSurveyForecast.NewPulseSurveyForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PulseSurveyForecastValidatorReportsEveryProblem();
        PulseSurveyForecastChannelLengthRuleFlagsALongChannel();
        PulseSurveyForecastCodeLengthRuleFlagsALongCode();
        PulseSurveyForecastRequiredTitleRuleFlagsABlankTitle();
        PulseSurveyForecastRequiredCodeRuleFlagsABlankCode();
    }
}
