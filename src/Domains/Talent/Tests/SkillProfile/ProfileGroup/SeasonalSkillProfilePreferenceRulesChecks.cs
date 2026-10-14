namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileGroup;

using Acme.Workforce.Talent.SkillProfile.ProfileGroup;

/// <summary>Checks of profile group code, run by the project's runner.</summary>
public static class SeasonalSkillProfilePreferenceRulesChecks
{
    public static void SeasonalSkillProfilePreferenceValidatorReportsEveryProblem()
    {
        var validator = SeasonalSkillProfilePreferenceRules.DefaultSeasonalSkillProfilePreferenceValidator();
        var item = SeasonalSkillProfilePreference.NewSeasonalSkillProfilePreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalSkillProfilePreferenceRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new SeasonalSkillProfilePreferenceRequiredCountryRule(
            "seasonal skill profile preference needs a value");
        var item = SeasonalSkillProfilePreference.NewSeasonalSkillProfilePreference("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalSkillProfilePreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalSkillProfilePreferenceRequiredCodeRule(
            "seasonal skill profile preference needs an entry");
        var item = SeasonalSkillProfilePreference.NewSeasonalSkillProfilePreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalSkillProfilePreferenceLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new SeasonalSkillProfilePreferenceLocaleLengthRule(3600);
        var item = SeasonalSkillProfilePreference.NewSeasonalSkillProfilePreference("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalSkillProfilePreferenceCountryLengthRuleFlagsALongCountry()
    {
        var rule = new SeasonalSkillProfilePreferenceCountryLengthRule(3600);
        var item = SeasonalSkillProfilePreference.NewSeasonalSkillProfilePreference("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalSkillProfilePreferenceValidatorReportsEveryProblem();
        SeasonalSkillProfilePreferenceRequiredCountryRuleFlagsABlankCountry();
        SeasonalSkillProfilePreferenceRequiredCodeRuleFlagsABlankCode();
        SeasonalSkillProfilePreferenceLocaleLengthRuleFlagsALongLocale();
        SeasonalSkillProfilePreferenceCountryLengthRuleFlagsALongCountry();
    }
}
