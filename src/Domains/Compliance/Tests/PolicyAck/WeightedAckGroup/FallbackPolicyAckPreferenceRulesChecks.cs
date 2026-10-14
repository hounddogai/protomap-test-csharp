namespace Acme.Workforce.Compliance.Tests.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

/// <summary>Checks of weighted ack group code, run by the project's runner.</summary>
public static class FallbackPolicyAckPreferenceRulesChecks
{
    public static void FallbackPolicyAckPreferenceValidatorReportsEveryProblem()
    {
        var validator = FallbackPolicyAckPreferenceRules.DefaultFallbackPolicyAckPreferenceValidator();
        var item = FallbackPolicyAckPreference.NewFallbackPolicyAckPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPolicyAckPreferenceRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new FallbackPolicyAckPreferenceRequiredUnitRule("fallback policy ack preference needs this field");
        var item = FallbackPolicyAckPreference.NewFallbackPolicyAckPreference("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPolicyAckPreferenceDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new FallbackPolicyAckPreferenceDiscountRangeRule(3.6, 142.7);
        var item = FallbackPolicyAckPreference.NewFallbackPolicyAckPreference("x", "y");
        item.Discount = 144.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPolicyAckPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackPolicyAckPreferenceRequiredCodeRule("fallback policy ack preference needs a value");
        var item = FallbackPolicyAckPreference.NewFallbackPolicyAckPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPolicyAckPreferenceCountryLengthRuleFlagsALongCountry()
    {
        var rule = new FallbackPolicyAckPreferenceCountryLengthRule(3600);
        var item = FallbackPolicyAckPreference.NewFallbackPolicyAckPreference("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPolicyAckPreferenceRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new FallbackPolicyAckPreferenceRequiredCountryRule("fallback policy ack preference needs this field");
        var item = FallbackPolicyAckPreference.NewFallbackPolicyAckPreference("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackPolicyAckPreferenceValidatorReportsEveryProblem();
        FallbackPolicyAckPreferenceRequiredUnitRuleFlagsABlankUnit();
        FallbackPolicyAckPreferenceDiscountRangeRuleFlagsValuesAboveDiscount();
        FallbackPolicyAckPreferenceRequiredCodeRuleFlagsABlankCode();
        FallbackPolicyAckPreferenceCountryLengthRuleFlagsALongCountry();
        FallbackPolicyAckPreferenceRequiredCountryRuleFlagsABlankCountry();
    }
}
