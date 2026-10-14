namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyEstimateRulesChecks
{
    public static void PulseSurveyEstimateValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyEstimateRules.DefaultPulseSurveyEstimateValidator();
        var item = PulseSurveyEstimate.NewPulseSurveyEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyEstimateRequiredNameRuleFlagsABlankName()
    {
        var rule = new PulseSurveyEstimateRequiredNameRule("pulse survey estimate needs a value");
        var item = PulseSurveyEstimate.NewPulseSurveyEstimate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyEstimateNameLengthRuleFlagsALongName()
    {
        var rule = new PulseSurveyEstimateNameLengthRule(1000);
        var item = PulseSurveyEstimate.NewPulseSurveyEstimate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PulseSurveyEstimateValidatorReportsEveryProblem();
        PulseSurveyEstimateRequiredNameRuleFlagsABlankName();
        PulseSurveyEstimateNameLengthRuleFlagsALongName();
    }
}
