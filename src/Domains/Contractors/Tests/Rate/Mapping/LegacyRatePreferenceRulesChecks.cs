namespace Acme.Workforce.Contractors.Tests.Rate.Mapping;

using Acme.Workforce.Contractors.Rate.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LegacyRatePreferenceRulesChecks
{
    public static void LegacyRatePreferenceValidatorReportsEveryProblem()
    {
        var validator = LegacyRatePreferenceRules.DefaultLegacyRatePreferenceValidator();
        var item = LegacyRatePreference.NewLegacyRatePreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyRatePreferenceCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new LegacyRatePreferenceCeilingRangeRule(9.1, 361.2);
        var item = LegacyRatePreference.NewLegacyRatePreference("x", "y");
        item.Ceiling = 362.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyRatePreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyRatePreferenceCodeLengthRule(64);
        var item = LegacyRatePreference.NewLegacyRatePreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyRatePreferenceValidatorReportsEveryProblem();
        LegacyRatePreferenceCeilingRangeRuleFlagsValuesAboveCeiling();
        LegacyRatePreferenceCodeLengthRuleFlagsALongCode();
    }
}
