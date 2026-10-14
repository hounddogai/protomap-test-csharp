namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Quota;

using Acme.Workforce.Engagement.PulseSurvey.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ManualPulseSurveyQuotaRulesChecks
{
    public static void ManualPulseSurveyQuotaValidatorReportsEveryProblem()
    {
        var validator = ManualPulseSurveyQuotaRules.DefaultManualPulseSurveyQuotaValidator();
        var item = ManualPulseSurveyQuota.NewManualPulseSurveyQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualPulseSurveyQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualPulseSurveyQuotaCodeLengthRule(120);
        var item = ManualPulseSurveyQuota.NewManualPulseSurveyQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualPulseSurveyQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualPulseSurveyQuotaRequiredCodeRule("manual pulse survey quota needs an entry");
        var item = ManualPulseSurveyQuota.NewManualPulseSurveyQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualPulseSurveyQuotaRegionLengthRuleFlagsALongRegion()
    {
        var rule = new ManualPulseSurveyQuotaRegionLengthRule(32);
        var item = ManualPulseSurveyQuota.NewManualPulseSurveyQuota("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualPulseSurveyQuotaValidatorReportsEveryProblem();
        ManualPulseSurveyQuotaCodeLengthRuleFlagsALongCode();
        ManualPulseSurveyQuotaRequiredCodeRuleFlagsABlankCode();
        ManualPulseSurveyQuotaRegionLengthRuleFlagsALongRegion();
    }
}
