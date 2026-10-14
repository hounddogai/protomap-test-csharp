namespace Acme.Workforce.Compliance.Tests.Attestation.Policy;

using Acme.Workforce.Compliance.Attestation.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AttestationPolicyRulesChecks
{
    public static void AttestationPolicyValidatorReportsEveryProblem()
    {
        var validator = AttestationPolicyRules.DefaultAttestationPolicyValidator();
        var item = AttestationPolicy.NewAttestationPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationPolicySequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new AttestationPolicySequenceRangeRule(16, 3600);
        var item = AttestationPolicy.NewAttestationPolicy("x", "y");
        item.Sequence = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationPolicyMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new AttestationPolicyMarginRangeRule(2.4, 384.1);
        var item = AttestationPolicy.NewAttestationPolicy("x", "y");
        item.Margin = 385.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationPolicyRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AttestationPolicyRequiredTitleRule("attestation policy needs this field");
        var item = AttestationPolicy.NewAttestationPolicy("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationPolicyDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new AttestationPolicyDiscountRangeRule(3.4, 2135.4);
        var item = AttestationPolicy.NewAttestationPolicy("x", "y");
        item.Discount = 2136.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AttestationPolicyRequiredCodeRule("attestation policy needs an entry");
        var item = AttestationPolicy.NewAttestationPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AttestationPolicyValidatorReportsEveryProblem();
        AttestationPolicySequenceRangeRuleFlagsValuesAboveSequence();
        AttestationPolicyMarginRangeRuleFlagsValuesAboveMargin();
        AttestationPolicyRequiredTitleRuleFlagsABlankTitle();
        AttestationPolicyDiscountRangeRuleFlagsValuesAboveDiscount();
        AttestationPolicyRequiredCodeRuleFlagsABlankCode();
    }
}
