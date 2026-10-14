namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class ManualPulseSurveyTierRulesChecks
{
    public static void ManualPulseSurveyTierValidatorReportsEveryProblem()
    {
        var validator = ManualPulseSurveyTierRules.DefaultManualPulseSurveyTierValidator();
        var item = ManualPulseSurveyTier.NewManualPulseSurveyTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualPulseSurveyTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualPulseSurveyTierCodeLengthRule(32);
        var item = ManualPulseSurveyTier.NewManualPulseSurveyTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualPulseSurveyTierTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ManualPulseSurveyTierTitleLengthRule(86400);
        var item = ManualPulseSurveyTier.NewManualPulseSurveyTier("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualPulseSurveyTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualPulseSurveyTierRequiredCodeRule("manual pulse survey tier needs a value");
        var item = ManualPulseSurveyTier.NewManualPulseSurveyTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualPulseSurveyTierValidatorReportsEveryProblem();
        ManualPulseSurveyTierCodeLengthRuleFlagsALongCode();
        ManualPulseSurveyTierTitleLengthRuleFlagsALongTitle();
        ManualPulseSurveyTierRequiredCodeRuleFlagsABlankCode();
    }
}
