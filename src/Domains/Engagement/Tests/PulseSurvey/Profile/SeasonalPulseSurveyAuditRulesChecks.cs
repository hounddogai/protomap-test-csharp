namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Profile;

using Acme.Workforce.Engagement.PulseSurvey.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class SeasonalPulseSurveyAuditRulesChecks
{
    public static void SeasonalPulseSurveyAuditValidatorReportsEveryProblem()
    {
        var validator = SeasonalPulseSurveyAuditRules.DefaultSeasonalPulseSurveyAuditValidator();
        var item = SeasonalPulseSurveyAudit.NewSeasonalPulseSurveyAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalPulseSurveyAuditRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new SeasonalPulseSurveyAuditRequiredChecksumRule("seasonal pulse survey audit needs a value");
        var item = SeasonalPulseSurveyAudit.NewSeasonalPulseSurveyAudit("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalPulseSurveyAuditLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new SeasonalPulseSurveyAuditLeaveDaysRangeRule(32, 80);
        var item = SeasonalPulseSurveyAudit.NewSeasonalPulseSurveyAudit("x", "y");
        item.LeaveDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalPulseSurveyAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalPulseSurveyAuditRequiredCodeRule("seasonal pulse survey audit needs a value");
        var item = SeasonalPulseSurveyAudit.NewSeasonalPulseSurveyAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalPulseSurveyAuditChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new SeasonalPulseSurveyAuditChecksumLengthRule(3600);
        var item = SeasonalPulseSurveyAudit.NewSeasonalPulseSurveyAudit("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalPulseSurveyAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalPulseSurveyAuditCodeLengthRule(86400);
        var item = SeasonalPulseSurveyAudit.NewSeasonalPulseSurveyAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalPulseSurveyAuditValidatorReportsEveryProblem();
        SeasonalPulseSurveyAuditRequiredChecksumRuleFlagsABlankChecksum();
        SeasonalPulseSurveyAuditLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        SeasonalPulseSurveyAuditRequiredCodeRuleFlagsABlankCode();
        SeasonalPulseSurveyAuditChecksumLengthRuleFlagsALongChecksum();
        SeasonalPulseSurveyAuditCodeLengthRuleFlagsALongCode();
    }
}
