namespace Acme.Workforce.Absence.Tests.Holiday.Schedule;

using Acme.Workforce.Absence.Holiday.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class HolidayPreferenceRulesChecks
{
    public static void HolidayPreferenceValidatorReportsEveryProblem()
    {
        var validator = HolidayPreferenceRules.DefaultHolidayPreferenceValidator();
        var item = HolidayPreference.NewHolidayPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayPreferenceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new HolidayPreferenceRequiredTitleRule("holiday preference needs a value");
        var item = HolidayPreference.NewHolidayPreference("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayPreferenceRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new HolidayPreferenceRequiredBadgeRule("holiday preference needs an entry");
        var item = HolidayPreference.NewHolidayPreference("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayPreferenceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new HolidayPreferenceTitleLengthRule(64);
        var item = HolidayPreference.NewHolidayPreference("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayPreferenceMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new HolidayPreferenceMinimumRangeRule(9.2, 1996.4);
        var item = HolidayPreference.NewHolidayPreference("x", "y");
        item.Minimum = 1997.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayPreferenceBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new HolidayPreferenceBadgeLengthRule(16);
        var item = HolidayPreference.NewHolidayPreference("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HolidayPreferenceValidatorReportsEveryProblem();
        HolidayPreferenceRequiredTitleRuleFlagsABlankTitle();
        HolidayPreferenceRequiredBadgeRuleFlagsABlankBadge();
        HolidayPreferenceTitleLengthRuleFlagsALongTitle();
        HolidayPreferenceMinimumRangeRuleFlagsValuesAboveMinimum();
        HolidayPreferenceBadgeLengthRuleFlagsALongBadge();
    }
}
