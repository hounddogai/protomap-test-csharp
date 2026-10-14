namespace Acme.Workforce.Talent.Tests.Mobility.Preference;

using Acme.Workforce.Talent.Mobility.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class MobilityPreferenceRulesChecks
{
    public static void MobilityPreferenceValidatorReportsEveryProblem()
    {
        var validator = MobilityPreferenceRules.DefaultMobilityPreferenceValidator();
        var item = MobilityPreference.NewMobilityPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityPreferenceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new MobilityPreferenceRequiredTitleRule("mobility preference needs this field");
        var item = MobilityPreference.NewMobilityPreference("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityPreferenceScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new MobilityPreferenceScoreRangeRule(3.3, 4004.9);
        var item = MobilityPreference.NewMobilityPreference("x", "y");
        item.Score = 4006.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityPreferenceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new MobilityPreferenceTitleLengthRule(86400);
        var item = MobilityPreference.NewMobilityPreference("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MobilityPreferenceValidatorReportsEveryProblem();
        MobilityPreferenceRequiredTitleRuleFlagsABlankTitle();
        MobilityPreferenceScoreRangeRuleFlagsValuesAboveScore();
        MobilityPreferenceTitleLengthRuleFlagsALongTitle();
    }
}
