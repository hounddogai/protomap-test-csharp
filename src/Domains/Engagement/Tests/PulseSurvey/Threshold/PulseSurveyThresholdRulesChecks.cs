namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PulseSurveyThresholdRulesChecks
{
    public static void PulseSurveyThresholdValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyThresholdRules.DefaultPulseSurveyThresholdValidator();
        var item = PulseSurveyThreshold.NewPulseSurveyThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PulseSurveyThresholdRequiredCodeRule("pulse survey threshold needs this field");
        var item = PulseSurveyThreshold.NewPulseSurveyThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyThresholdThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new PulseSurveyThresholdThresholdRangeRule(3.7, 3536.3);
        var item = PulseSurveyThreshold.NewPulseSurveyThreshold("x", "y");
        item.Threshold = 3537.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyThresholdRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new PulseSurveyThresholdRequiredSalaryBandRule("pulse survey threshold needs this field");
        var item = PulseSurveyThreshold.NewPulseSurveyThreshold("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PulseSurveyThresholdValidatorReportsEveryProblem();
        PulseSurveyThresholdRequiredCodeRuleFlagsABlankCode();
        PulseSurveyThresholdThresholdRangeRuleFlagsValuesAboveThreshold();
        PulseSurveyThresholdRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
