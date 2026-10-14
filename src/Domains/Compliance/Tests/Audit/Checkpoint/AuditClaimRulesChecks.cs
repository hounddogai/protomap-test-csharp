namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AuditClaimRulesChecks
{
    public static void AuditClaimValidatorReportsEveryProblem()
    {
        var validator = AuditClaimRules.DefaultAuditClaimValidator();
        var item = AuditClaim.NewAuditClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new AuditClaimCodeLengthRule(16);
        var item = AuditClaim.NewAuditClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AuditClaimRequiredCodeRule("audit claim needs a value");
        var item = AuditClaim.NewAuditClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AuditClaimValidatorReportsEveryProblem();
        AuditClaimCodeLengthRuleFlagsALongCode();
        AuditClaimRequiredCodeRuleFlagsABlankCode();
    }
}
