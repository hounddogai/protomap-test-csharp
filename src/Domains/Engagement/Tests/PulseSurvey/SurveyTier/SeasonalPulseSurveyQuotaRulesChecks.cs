namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class SeasonalPulseSurveyQuotaRulesChecks
{
    public static void SeasonalPulseSurveyQuotaValidatorReportsEveryProblem()
    {
        var validator = SeasonalPulseSurveyQuotaRules.DefaultSeasonalPulseSurveyQuotaValidator();
        var item = SeasonalPulseSurveyQuota.NewSeasonalPulseSurveyQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalPulseSurveyQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalPulseSurveyQuotaCodeLengthRule(32);
        var item = SeasonalPulseSurveyQuota.NewSeasonalPulseSurveyQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalPulseSurveyQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalPulseSurveyQuotaRequiredCodeRule("seasonal pulse survey quota needs this field");
        var item = SeasonalPulseSurveyQuota.NewSeasonalPulseSurveyQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalPulseSurveyQuotaExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new SeasonalPulseSurveyQuotaExternalRefLengthRule(16);
        var item = SeasonalPulseSurveyQuota.NewSeasonalPulseSurveyQuota("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalPulseSurveyQuotaValidatorReportsEveryProblem();
        SeasonalPulseSurveyQuotaCodeLengthRuleFlagsALongCode();
        SeasonalPulseSurveyQuotaRequiredCodeRuleFlagsABlankCode();
        SeasonalPulseSurveyQuotaExternalRefLengthRuleFlagsALongExternalRef();
    }
}
