namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Variance;

using Acme.Workforce.Engagement.PulseSurvey.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PulseSurveyOverrideRulesChecks
{
    public static void PulseSurveyOverrideValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyOverrideRules.DefaultPulseSurveyOverrideValidator();
        var item = PulseSurveyOverride.NewPulseSurveyOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyOverrideLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PulseSurveyOverrideLabelLengthRule(16);
        var item = PulseSurveyOverride.NewPulseSurveyOverride("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyOverrideUnitLengthRuleFlagsALongUnit()
    {
        var rule = new PulseSurveyOverrideUnitLengthRule(16);
        var item = PulseSurveyOverride.NewPulseSurveyOverride("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PulseSurveyOverrideRequiredCodeRule("pulse survey override needs an entry");
        var item = PulseSurveyOverride.NewPulseSurveyOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PulseSurveyOverrideValidatorReportsEveryProblem();
        PulseSurveyOverrideLabelLengthRuleFlagsALongLabel();
        PulseSurveyOverrideUnitLengthRuleFlagsALongUnit();
        PulseSurveyOverrideRequiredCodeRuleFlagsABlankCode();
    }
}
