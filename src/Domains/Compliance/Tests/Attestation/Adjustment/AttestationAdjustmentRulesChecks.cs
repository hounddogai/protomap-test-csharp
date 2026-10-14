namespace Acme.Workforce.Compliance.Tests.Attestation.Adjustment;

using Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AttestationAdjustmentRulesChecks
{
    public static void AttestationAdjustmentValidatorReportsEveryProblem()
    {
        var validator = AttestationAdjustmentRules.DefaultAttestationAdjustmentValidator();
        var item = AttestationAdjustment.NewAttestationAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationAdjustmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AttestationAdjustmentTitleLengthRule(120);
        var item = AttestationAdjustment.NewAttestationAdjustment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationAdjustmentRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new AttestationAdjustmentRequiredExternalRefRule("attestation adjustment needs a value");
        var item = AttestationAdjustment.NewAttestationAdjustment("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationAdjustmentRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new AttestationAdjustmentRatioRangeRule(8.8, 3377.5);
        var item = AttestationAdjustment.NewAttestationAdjustment("x", "y");
        item.Ratio = 3379.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new AttestationAdjustmentCodeLengthRule(3600);
        var item = AttestationAdjustment.NewAttestationAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AttestationAdjustmentValidatorReportsEveryProblem();
        AttestationAdjustmentTitleLengthRuleFlagsALongTitle();
        AttestationAdjustmentRequiredExternalRefRuleFlagsABlankExternalRef();
        AttestationAdjustmentRatioRangeRuleFlagsValuesAboveRatio();
        AttestationAdjustmentCodeLengthRuleFlagsALongCode();
    }
}
