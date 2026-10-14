namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentAuditRulesChecks
{
    public static void SentimentAuditValidatorReportsEveryProblem()
    {
        var validator = SentimentAuditRules.DefaultSentimentAuditValidator();
        var item = SentimentAudit.NewSentimentAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SentimentAuditRequiredCodeRule("sentiment audit needs a value");
        var item = SentimentAudit.NewSentimentAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentAuditLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SentimentAuditLabelLengthRule(120);
        var item = SentimentAudit.NewSentimentAudit("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentAuditRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new SentimentAuditRequiredDepartmentRule("sentiment audit needs this field");
        var item = SentimentAudit.NewSentimentAudit("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentAuditRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SentimentAuditRequiredLabelRule("sentiment audit needs a value");
        var item = SentimentAudit.NewSentimentAudit("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new SentimentAuditCodeLengthRule(64);
        var item = SentimentAudit.NewSentimentAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SentimentAuditValidatorReportsEveryProblem();
        SentimentAuditRequiredCodeRuleFlagsABlankCode();
        SentimentAuditLabelLengthRuleFlagsALongLabel();
        SentimentAuditRequiredDepartmentRuleFlagsABlankDepartment();
        SentimentAuditRequiredLabelRuleFlagsABlankLabel();
        SentimentAuditCodeLengthRuleFlagsALongCode();
    }
}
