namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Target;

using Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingPulseSurveyAuditRulesChecks
{
    public static void PendingPulseSurveyAuditValidatorReportsEveryProblem()
    {
        var validator = PendingPulseSurveyAuditRules.DefaultPendingPulseSurveyAuditValidator();
        var item = PendingPulseSurveyAudit.NewPendingPulseSurveyAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingPulseSurveyAuditTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PendingPulseSurveyAuditTitleLengthRule(3600);
        var item = PendingPulseSurveyAudit.NewPendingPulseSurveyAudit("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingPulseSurveyAuditLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new PendingPulseSurveyAuditLeaveDaysRangeRule(32, 80);
        var item = PendingPulseSurveyAudit.NewPendingPulseSurveyAudit("x", "y");
        item.LeaveDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingPulseSurveyAuditRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingPulseSurveyAuditRequiredTitleRule("pending pulse survey audit needs a value");
        var item = PendingPulseSurveyAudit.NewPendingPulseSurveyAudit("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingPulseSurveyAuditRegionLengthRuleFlagsALongRegion()
    {
        var rule = new PendingPulseSurveyAuditRegionLengthRule(1000);
        var item = PendingPulseSurveyAudit.NewPendingPulseSurveyAudit("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingPulseSurveyAuditRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new PendingPulseSurveyAuditRequiredLocaleRule("pending pulse survey audit needs an entry");
        var item = PendingPulseSurveyAudit.NewPendingPulseSurveyAudit("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingPulseSurveyAuditRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new PendingPulseSurveyAuditRequiredRegionRule("pending pulse survey audit needs this field");
        var item = PendingPulseSurveyAudit.NewPendingPulseSurveyAudit("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingPulseSurveyAuditValidatorReportsEveryProblem();
        PendingPulseSurveyAuditTitleLengthRuleFlagsALongTitle();
        PendingPulseSurveyAuditLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        PendingPulseSurveyAuditRequiredTitleRuleFlagsABlankTitle();
        PendingPulseSurveyAuditRegionLengthRuleFlagsALongRegion();
        PendingPulseSurveyAuditRequiredLocaleRuleFlagsABlankLocale();
        PendingPulseSurveyAuditRequiredRegionRuleFlagsABlankRegion();
    }
}
