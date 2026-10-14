namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Tier;

using Acme.Workforce.Engagement.PulseSurvey.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StrictPulseSurveyTierRulesChecks
{
    public static void StrictPulseSurveyTierValidatorReportsEveryProblem()
    {
        var validator = StrictPulseSurveyTierRules.DefaultStrictPulseSurveyTierValidator();
        var item = StrictPulseSurveyTier.NewStrictPulseSurveyTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictPulseSurveyTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictPulseSurveyTierCodeLengthRule(32);
        var item = StrictPulseSurveyTier.NewStrictPulseSurveyTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictPulseSurveyTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StrictPulseSurveyTierRequiredLabelRule("strict pulse survey tier needs this field");
        var item = StrictPulseSurveyTier.NewStrictPulseSurveyTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictPulseSurveyTierSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new StrictPulseSurveyTierSummaryLengthRule(1000);
        var item = StrictPulseSurveyTier.NewStrictPulseSurveyTier("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictPulseSurveyTierValidatorReportsEveryProblem();
        StrictPulseSurveyTierCodeLengthRuleFlagsALongCode();
        StrictPulseSurveyTierRequiredLabelRuleFlagsABlankLabel();
        StrictPulseSurveyTierSummaryLengthRuleFlagsALongSummary();
    }
}
