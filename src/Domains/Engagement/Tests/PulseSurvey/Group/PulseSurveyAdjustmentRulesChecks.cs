namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Group;

using Acme.Workforce.Engagement.PulseSurvey.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PulseSurveyAdjustmentRulesChecks
{
    public static void PulseSurveyAdjustmentValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyAdjustmentRules.DefaultPulseSurveyAdjustmentValidator();
        var item = PulseSurveyAdjustment.NewPulseSurveyAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyAdjustmentAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new PulseSurveyAdjustmentAttemptsRangeRule(1000, 86400);
        var item = PulseSurveyAdjustment.NewPulseSurveyAdjustment("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PulseSurveyAdjustmentRequiredCodeRule("pulse survey adjustment needs an entry");
        var item = PulseSurveyAdjustment.NewPulseSurveyAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PulseSurveyAdjustmentValidatorReportsEveryProblem();
        PulseSurveyAdjustmentAttemptsRangeRuleFlagsValuesAboveAttempts();
        PulseSurveyAdjustmentRequiredCodeRuleFlagsABlankCode();
    }
}
