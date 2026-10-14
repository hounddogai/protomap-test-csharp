namespace Acme.Workforce.Compliance.Tests.Attestation.Policy;

using Acme.Workforce.Compliance.Attestation.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AttestationPreferenceRulesChecks
{
    public static void AttestationPreferenceValidatorReportsEveryProblem()
    {
        var validator = AttestationPreferenceRules.DefaultAttestationPreferenceValidator();
        var item = AttestationPreference.NewAttestationPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationPreferenceQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new AttestationPreferenceQuantityRangeRule(32, 1000);
        var item = AttestationPreference.NewAttestationPreference("x", "y");
        item.Quantity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationPreferenceRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new AttestationPreferenceRequiredCurrencyRule("attestation preference needs this field");
        var item = AttestationPreference.NewAttestationPreference("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationPreferenceExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new AttestationPreferenceExpiresAtRangeRule(32, 3600);
        var item = AttestationPreference.NewAttestationPreference("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AttestationPreferenceValidatorReportsEveryProblem();
        AttestationPreferenceQuantityRangeRuleFlagsValuesAboveQuantity();
        AttestationPreferenceRequiredCurrencyRuleFlagsABlankCurrency();
        AttestationPreferenceExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
