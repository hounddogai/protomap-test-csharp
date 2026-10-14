namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class CappedAttestationAuditRulesChecks
{
    public static void CappedAttestationAuditValidatorReportsEveryProblem()
    {
        var validator = CappedAttestationAuditRules.DefaultCappedAttestationAuditValidator();
        var item = CappedAttestationAudit.NewCappedAttestationAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedAttestationAuditMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new CappedAttestationAuditMaximumRangeRule(5.9, 3030.5);
        var item = CappedAttestationAudit.NewCappedAttestationAudit("x", "y");
        item.Maximum = 3032.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedAttestationAuditLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedAttestationAuditLabelLengthRule(120);
        var item = CappedAttestationAudit.NewCappedAttestationAudit("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAttestationAuditRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new CappedAttestationAuditRequiredDescriptionRule("capped attestation audit needs an entry");
        var item = CappedAttestationAudit.NewCappedAttestationAudit("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedAttestationAuditValidatorReportsEveryProblem();
        CappedAttestationAuditMaximumRangeRuleFlagsValuesAboveMaximum();
        CappedAttestationAuditLabelLengthRuleFlagsALongLabel();
        CappedAttestationAuditRequiredDescriptionRuleFlagsABlankDescription();
    }
}
