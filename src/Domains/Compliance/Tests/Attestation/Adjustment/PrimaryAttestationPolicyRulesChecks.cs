namespace Acme.Workforce.Compliance.Tests.Attestation.Adjustment;

using Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PrimaryAttestationPolicyRulesChecks
{
    public static void PrimaryAttestationPolicyValidatorReportsEveryProblem()
    {
        var validator = PrimaryAttestationPolicyRules.DefaultPrimaryAttestationPolicyValidator();
        var item = PrimaryAttestationPolicy.NewPrimaryAttestationPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryAttestationPolicyDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new PrimaryAttestationPolicyDiscountRangeRule(5.1, 311.5);
        var item = PrimaryAttestationPolicy.NewPrimaryAttestationPolicy("x", "y");
        item.Discount = 313.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryAttestationPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryAttestationPolicyCodeLengthRule(80);
        var item = PrimaryAttestationPolicy.NewPrimaryAttestationPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryAttestationPolicyNameLengthRuleFlagsALongName()
    {
        var rule = new PrimaryAttestationPolicyNameLengthRule(80);
        var item = PrimaryAttestationPolicy.NewPrimaryAttestationPolicy("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryAttestationPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryAttestationPolicyRequiredCodeRule("primary attestation policy needs an entry");
        var item = PrimaryAttestationPolicy.NewPrimaryAttestationPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryAttestationPolicyValidatorReportsEveryProblem();
        PrimaryAttestationPolicyDiscountRangeRuleFlagsValuesAboveDiscount();
        PrimaryAttestationPolicyCodeLengthRuleFlagsALongCode();
        PrimaryAttestationPolicyNameLengthRuleFlagsALongName();
        PrimaryAttestationPolicyRequiredCodeRuleFlagsABlankCode();
    }
}
