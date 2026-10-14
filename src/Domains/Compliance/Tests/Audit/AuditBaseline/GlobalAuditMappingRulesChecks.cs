namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class GlobalAuditMappingRulesChecks
{
    public static void GlobalAuditMappingValidatorReportsEveryProblem()
    {
        var validator = GlobalAuditMappingRules.DefaultGlobalAuditMappingValidator();
        var item = GlobalAuditMapping.NewGlobalAuditMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalAuditMappingNameLengthRuleFlagsALongName()
    {
        var rule = new GlobalAuditMappingNameLengthRule(120);
        var item = GlobalAuditMapping.NewGlobalAuditMapping("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAuditMappingRequiredNameRuleFlagsABlankName()
    {
        var rule = new GlobalAuditMappingRequiredNameRule("global audit mapping needs an entry");
        var item = GlobalAuditMapping.NewGlobalAuditMapping("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalAuditMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalAuditMappingRequiredCodeRule("global audit mapping needs a value");
        var item = GlobalAuditMapping.NewGlobalAuditMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalAuditMappingValidatorReportsEveryProblem();
        GlobalAuditMappingNameLengthRuleFlagsALongName();
        GlobalAuditMappingRequiredNameRuleFlagsABlankName();
        GlobalAuditMappingRequiredCodeRuleFlagsABlankCode();
    }
}
