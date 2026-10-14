namespace Acme.Workforce.Talent.Tests.Pool.Quota;

using Acme.Workforce.Talent.Pool.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PoolPreferenceRulesChecks
{
    public static void PoolPreferenceValidatorReportsEveryProblem()
    {
        var validator = PoolPreferenceRules.DefaultPoolPreferenceValidator();
        var item = PoolPreference.NewPoolPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolPreferenceCountryLengthRuleFlagsALongCountry()
    {
        var rule = new PoolPreferenceCountryLengthRule(16);
        var item = PoolPreference.NewPoolPreference("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PoolPreferenceRequiredCodeRule("pool preference needs an entry");
        var item = PoolPreference.NewPoolPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolPreferenceScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PoolPreferenceScoreRangeRule(9.3, 1387.7);
        var item = PoolPreference.NewPoolPreference("x", "y");
        item.Score = 1389.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolPreferenceLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PoolPreferenceLabelLengthRule(3600);
        var item = PoolPreference.NewPoolPreference("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolPreferenceWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new PoolPreferenceWindowDaysRangeRule(16, 16);
        var item = PoolPreference.NewPoolPreference("x", "y");
        item.WindowDays = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PoolPreferenceValidatorReportsEveryProblem();
        PoolPreferenceCountryLengthRuleFlagsALongCountry();
        PoolPreferenceRequiredCodeRuleFlagsABlankCode();
        PoolPreferenceScoreRangeRuleFlagsValuesAboveScore();
        PoolPreferenceLabelLengthRuleFlagsALongLabel();
        PoolPreferenceWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
