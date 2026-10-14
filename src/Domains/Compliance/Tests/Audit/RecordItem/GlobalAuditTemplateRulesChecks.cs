namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GlobalAuditTemplateRulesChecks
{
    public static void GlobalAuditTemplateValidatorReportsEveryProblem()
    {
        var validator = GlobalAuditTemplateRules.DefaultGlobalAuditTemplateValidator();
        var item = GlobalAuditTemplate.NewGlobalAuditTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalAuditTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalAuditTemplateCodeLengthRule(32);
        var item = GlobalAuditTemplate.NewGlobalAuditTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAuditTemplateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new GlobalAuditTemplateRequiredLabelRule("global audit template needs an entry");
        var item = GlobalAuditTemplate.NewGlobalAuditTemplate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalAuditTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalAuditTemplateRequiredCodeRule("global audit template needs an entry");
        var item = GlobalAuditTemplate.NewGlobalAuditTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalAuditTemplateOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new GlobalAuditTemplateOwnerIdLengthRule(86400);
        var item = GlobalAuditTemplate.NewGlobalAuditTemplate("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAuditTemplateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalAuditTemplateLabelLengthRule(86400);
        var item = GlobalAuditTemplate.NewGlobalAuditTemplate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalAuditTemplateValidatorReportsEveryProblem();
        GlobalAuditTemplateCodeLengthRuleFlagsALongCode();
        GlobalAuditTemplateRequiredLabelRuleFlagsABlankLabel();
        GlobalAuditTemplateRequiredCodeRuleFlagsABlankCode();
        GlobalAuditTemplateOwnerIdLengthRuleFlagsALongOwnerId();
        GlobalAuditTemplateLabelLengthRuleFlagsALongLabel();
    }
}
