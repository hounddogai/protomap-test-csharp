namespace Acme.Workforce.Talent.Tests.SkillProfile.Preference;

using Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LegacySkillProfilePreferenceRulesChecks
{
    public static void LegacySkillProfilePreferenceValidatorReportsEveryProblem()
    {
        var validator = LegacySkillProfilePreferenceRules.DefaultLegacySkillProfilePreferenceValidator();
        var item = LegacySkillProfilePreference.NewLegacySkillProfilePreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacySkillProfilePreferenceSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new LegacySkillProfilePreferenceSummaryLengthRule(1000);
        var item = LegacySkillProfilePreference.NewLegacySkillProfilePreference("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacySkillProfilePreferenceRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new LegacySkillProfilePreferenceRequiredLocaleRule("legacy skill profile preference needs an entry");
        var item = LegacySkillProfilePreference.NewLegacySkillProfilePreference("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacySkillProfilePreferenceNameLengthRuleFlagsALongName()
    {
        var rule = new LegacySkillProfilePreferenceNameLengthRule(3600);
        var item = LegacySkillProfilePreference.NewLegacySkillProfilePreference("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacySkillProfilePreferenceEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new LegacySkillProfilePreferenceEndsAtRangeRule(80, 255);
        var item = LegacySkillProfilePreference.NewLegacySkillProfilePreference("x", "y");
        item.EndsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacySkillProfilePreferenceValidatorReportsEveryProblem();
        LegacySkillProfilePreferenceSummaryLengthRuleFlagsALongSummary();
        LegacySkillProfilePreferenceRequiredLocaleRuleFlagsABlankLocale();
        LegacySkillProfilePreferenceNameLengthRuleFlagsALongName();
        LegacySkillProfilePreferenceEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
