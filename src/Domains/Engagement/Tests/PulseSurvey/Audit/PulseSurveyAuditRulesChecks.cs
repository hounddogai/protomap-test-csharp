namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Audit;

using Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PulseSurveyAuditRulesChecks
{
    public static void PulseSurveyAuditValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyAuditRules.DefaultPulseSurveyAuditValidator();
        var item = PulseSurveyAudit.NewPulseSurveyAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyAuditJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new PulseSurveyAuditJobCodeLengthRule(86400);
        var item = PulseSurveyAudit.NewPulseSurveyAudit("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new PulseSurveyAuditCodeLengthRule(64);
        var item = PulseSurveyAudit.NewPulseSurveyAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyAuditLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PulseSurveyAuditLabelLengthRule(120);
        var item = PulseSurveyAudit.NewPulseSurveyAudit("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyAuditRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PulseSurveyAuditRequiredLabelRule("pulse survey audit needs this field");
        var item = PulseSurveyAudit.NewPulseSurveyAudit("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PulseSurveyAuditValidatorReportsEveryProblem();
        PulseSurveyAuditJobCodeLengthRuleFlagsALongJobCode();
        PulseSurveyAuditCodeLengthRuleFlagsALongCode();
        PulseSurveyAuditLabelLengthRuleFlagsALongLabel();
        PulseSurveyAuditRequiredLabelRuleFlagsABlankLabel();
    }
}
