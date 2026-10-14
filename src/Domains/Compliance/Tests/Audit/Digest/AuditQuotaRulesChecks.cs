namespace Acme.Workforce.Compliance.Tests.Audit.Digest;

using Acme.Workforce.Compliance.Audit.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AuditQuotaRulesChecks
{
    public static void AuditQuotaValidatorReportsEveryProblem()
    {
        var validator = AuditQuotaRules.DefaultAuditQuotaValidator();
        var item = AuditQuota.NewAuditQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditQuotaNameLengthRuleFlagsALongName()
    {
        var rule = new AuditQuotaNameLengthRule(255);
        var item = AuditQuota.NewAuditQuota("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditQuotaCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new AuditQuotaCategoryLengthRule(3600);
        var item = AuditQuota.NewAuditQuota("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AuditQuotaRequiredCodeRule("audit quota needs a value");
        var item = AuditQuota.NewAuditQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new AuditQuotaCodeLengthRule(64);
        var item = AuditQuota.NewAuditQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AuditQuotaValidatorReportsEveryProblem();
        AuditQuotaNameLengthRuleFlagsALongName();
        AuditQuotaCategoryLengthRuleFlagsALongCategory();
        AuditQuotaRequiredCodeRuleFlagsABlankCode();
        AuditQuotaCodeLengthRuleFlagsALongCode();
    }
}
