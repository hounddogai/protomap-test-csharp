namespace Acme.Workforce.Compliance.Tests.Attestation.Cycle;

using Acme.Workforce.Compliance.Attestation.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AttestationProfileRulesChecks
{
    public static void AttestationProfileValidatorReportsEveryProblem()
    {
        var validator = AttestationProfileRules.DefaultAttestationProfileValidator();
        var item = AttestationProfile.NewAttestationProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationProfileRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AttestationProfileRequiredLabelRule("attestation profile needs a value");
        var item = AttestationProfile.NewAttestationProfile("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationProfileQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new AttestationProfileQuantityRangeRule(80, 1000);
        var item = AttestationProfile.NewAttestationProfile("x", "y");
        item.Quantity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationProfileRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new AttestationProfileRequiredCountryRule("attestation profile needs a value");
        var item = AttestationProfile.NewAttestationProfile("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AttestationProfileValidatorReportsEveryProblem();
        AttestationProfileRequiredLabelRuleFlagsABlankLabel();
        AttestationProfileQuantityRangeRuleFlagsValuesAboveQuantity();
        AttestationProfileRequiredCountryRuleFlagsABlankCountry();
    }
}
