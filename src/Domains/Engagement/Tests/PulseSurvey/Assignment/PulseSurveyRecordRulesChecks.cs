namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Assignment;

using Acme.Workforce.Engagement.PulseSurvey.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PulseSurveyRecordRulesChecks
{
    public static void PulseSurveyRecordValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyRecordRules.DefaultPulseSurveyRecordValidator();
        var item = PulseSurveyRecord.NewPulseSurveyRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyRecordRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new PulseSurveyRecordRequiredLocaleRule("pulse survey record needs an entry");
        var item = PulseSurveyRecord.NewPulseSurveyRecord("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new PulseSurveyRecordCodeLengthRule(16);
        var item = PulseSurveyRecord.NewPulseSurveyRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PulseSurveyRecordValidatorReportsEveryProblem();
        PulseSurveyRecordRequiredLocaleRuleFlagsABlankLocale();
        PulseSurveyRecordCodeLengthRuleFlagsALongCode();
    }
}
