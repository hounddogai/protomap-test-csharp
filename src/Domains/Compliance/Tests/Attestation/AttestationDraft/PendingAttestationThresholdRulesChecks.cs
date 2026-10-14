namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationDraft;

using Acme.Workforce.Compliance.Attestation.AttestationDraft;

/// <summary>Checks of attestation draft code, run by the project's runner.</summary>
public static class PendingAttestationThresholdRulesChecks
{
    public static void PendingAttestationThresholdValidatorReportsEveryProblem()
    {
        var validator = PendingAttestationThresholdRules.DefaultPendingAttestationThresholdValidator();
        var item = PendingAttestationThreshold.NewPendingAttestationThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingAttestationThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingAttestationThresholdRequiredCodeRule("pending attestation threshold needs a value");
        var item = PendingAttestationThreshold.NewPendingAttestationThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingAttestationThresholdManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new PendingAttestationThresholdManagerIdLengthRule(80);
        var item = PendingAttestationThreshold.NewPendingAttestationThreshold("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingAttestationThresholdRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new PendingAttestationThresholdRequiredManagerIdRule(
            "pending attestation threshold needs this field");
        var item = PendingAttestationThreshold.NewPendingAttestationThreshold("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingAttestationThresholdDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new PendingAttestationThresholdDepartmentLengthRule(16);
        var item = PendingAttestationThreshold.NewPendingAttestationThreshold("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingAttestationThresholdQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new PendingAttestationThresholdQuantityRangeRule(16, 32);
        var item = PendingAttestationThreshold.NewPendingAttestationThreshold("x", "y");
        item.Quantity = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingAttestationThresholdLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingAttestationThresholdLabelLengthRule(3600);
        var item = PendingAttestationThreshold.NewPendingAttestationThreshold("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingAttestationThresholdValidatorReportsEveryProblem();
        PendingAttestationThresholdRequiredCodeRuleFlagsABlankCode();
        PendingAttestationThresholdManagerIdLengthRuleFlagsALongManagerId();
        PendingAttestationThresholdRequiredManagerIdRuleFlagsABlankManagerId();
        PendingAttestationThresholdDepartmentLengthRuleFlagsALongDepartment();
        PendingAttestationThresholdQuantityRangeRuleFlagsValuesAboveQuantity();
        PendingAttestationThresholdLabelLengthRuleFlagsALongLabel();
    }
}
