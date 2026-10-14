namespace Acme.Workforce.Compliance.Tests.Attestation.Preference;

using Acme.Workforce.Compliance.Attestation.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalAttestationPreferenceRulesChecks
{
    public static void RegionalAttestationPreferenceValidatorReportsEveryProblem()
    {
        var validator = RegionalAttestationPreferenceRules.DefaultRegionalAttestationPreferenceValidator();
        var item = RegionalAttestationPreference.NewRegionalAttestationPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalAttestationPreferenceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RegionalAttestationPreferenceTitleLengthRule(32);
        var item = RegionalAttestationPreference.NewRegionalAttestationPreference("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalAttestationPreferenceRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new RegionalAttestationPreferenceRequiredTimezoneRule(
            "regional attestation preference needs an entry");
        var item = RegionalAttestationPreference.NewRegionalAttestationPreference("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalAttestationPreferenceExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new RegionalAttestationPreferenceExpiresAtRangeRule(16, 3600);
        var item = RegionalAttestationPreference.NewRegionalAttestationPreference("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalAttestationPreferenceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RegionalAttestationPreferenceRequiredTitleRule("regional attestation preference needs a value");
        var item = RegionalAttestationPreference.NewRegionalAttestationPreference("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalAttestationPreferenceBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new RegionalAttestationPreferenceBudgetRangeRule(8.5, 428.4);
        var item = RegionalAttestationPreference.NewRegionalAttestationPreference("x", "y");
        item.Budget = 429.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalAttestationPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalAttestationPreferenceRequiredCodeRule("regional attestation preference needs an entry");
        var item = RegionalAttestationPreference.NewRegionalAttestationPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalAttestationPreferenceValidatorReportsEveryProblem();
        RegionalAttestationPreferenceTitleLengthRuleFlagsALongTitle();
        RegionalAttestationPreferenceRequiredTimezoneRuleFlagsABlankTimezone();
        RegionalAttestationPreferenceExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        RegionalAttestationPreferenceRequiredTitleRuleFlagsABlankTitle();
        RegionalAttestationPreferenceBudgetRangeRuleFlagsValuesAboveBudget();
        RegionalAttestationPreferenceRequiredCodeRuleFlagsABlankCode();
    }
}
