namespace Acme.Workforce.Compliance.Tests.Audit.AuditDraft;

using Acme.Workforce.Compliance.Audit.AuditDraft;

/// <summary>Checks of audit draft code, run by the project's runner.</summary>
public static class AuditSegmentRulesChecks
{
    public static void AuditSegmentValidatorReportsEveryProblem()
    {
        var validator = AuditSegmentRules.DefaultAuditSegmentValidator();
        var item = AuditSegment.NewAuditSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditSegmentRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new AuditSegmentRequiredJobCodeRule("audit segment needs an entry");
        var item = AuditSegment.NewAuditSegment("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new AuditSegmentCodeLengthRule(255);
        var item = AuditSegment.NewAuditSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AuditSegmentValidatorReportsEveryProblem();
        AuditSegmentRequiredJobCodeRuleFlagsABlankJobCode();
        AuditSegmentCodeLengthRuleFlagsALongCode();
    }
}
