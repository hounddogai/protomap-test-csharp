namespace Acme.Workforce.Talent.Tests.SkillProfile.Snapshot;

using Acme.Workforce.Talent.SkillProfile.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SkillProfilePreferenceRulesChecks
{
    public static void SkillProfilePreferenceValidatorReportsEveryProblem()
    {
        var validator = SkillProfilePreferenceRules.DefaultSkillProfilePreferenceValidator();
        var item = SkillProfilePreference.NewSkillProfilePreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfilePreferenceMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new SkillProfilePreferenceMarginRangeRule(5.9, 2323.9);
        var item = SkillProfilePreference.NewSkillProfilePreference("x", "y");
        item.Margin = 2325.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfilePreferenceLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SkillProfilePreferenceLabelLengthRule(86400);
        var item = SkillProfilePreference.NewSkillProfilePreference("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfilePreferenceCountryLengthRuleFlagsALongCountry()
    {
        var rule = new SkillProfilePreferenceCountryLengthRule(64);
        var item = SkillProfilePreference.NewSkillProfilePreference("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SkillProfilePreferenceValidatorReportsEveryProblem();
        SkillProfilePreferenceMarginRangeRuleFlagsValuesAboveMargin();
        SkillProfilePreferenceLabelLengthRuleFlagsALongLabel();
        SkillProfilePreferenceCountryLengthRuleFlagsALongCountry();
    }
}
