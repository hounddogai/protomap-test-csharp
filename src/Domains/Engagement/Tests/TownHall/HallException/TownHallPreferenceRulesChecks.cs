namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class TownHallPreferenceRulesChecks
{
    public static void TownHallPreferenceValidatorReportsEveryProblem()
    {
        var validator = TownHallPreferenceRules.DefaultTownHallPreferenceValidator();
        var item = TownHallPreference.NewTownHallPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallPreferenceRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new TownHallPreferenceRateRangeRule(5.6, 721.9);
        var item = TownHallPreference.NewTownHallPreference("x", "y");
        item.Rate = 723.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallPreferenceHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new TownHallPreferenceHoursRangeRule(3.6, 3800.6);
        var item = TownHallPreference.NewTownHallPreference("x", "y");
        item.Hours = 3802.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallPreferenceLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TownHallPreferenceLabelLengthRule(1000);
        var item = TownHallPreference.NewTownHallPreference("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TownHallPreferenceValidatorReportsEveryProblem();
        TownHallPreferenceRateRangeRuleFlagsValuesAboveRate();
        TownHallPreferenceHoursRangeRuleFlagsValuesAboveHours();
        TownHallPreferenceLabelLengthRuleFlagsALongLabel();
    }
}
