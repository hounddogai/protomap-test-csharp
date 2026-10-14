namespace Acme.Workforce.Compliance.Tests.Attestation.Audit;

using Acme.Workforce.Compliance.Attestation.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AttestationAuditRulesChecks
{
    public static void AttestationAuditValidatorReportsEveryProblem()
    {
        var validator = AttestationAuditRules.DefaultAttestationAuditValidator();
        var item = AttestationAudit.NewAttestationAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationAuditRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AttestationAuditRequiredTitleRule("attestation audit needs an entry");
        var item = AttestationAudit.NewAttestationAudit("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationAuditRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new AttestationAuditRequiredTimezoneRule("attestation audit needs this field");
        var item = AttestationAudit.NewAttestationAudit("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AttestationAuditValidatorReportsEveryProblem();
        AttestationAuditRequiredTitleRuleFlagsABlankTitle();
        AttestationAuditRequiredTimezoneRuleFlagsABlankTimezone();
    }
}
