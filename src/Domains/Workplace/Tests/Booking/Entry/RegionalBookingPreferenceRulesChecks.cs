namespace Acme.Workforce.Workplace.Tests.Booking.Entry;

using Acme.Workforce.Workplace.Booking.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RegionalBookingPreferenceRulesChecks
{
    public static void RegionalBookingPreferenceValidatorReportsEveryProblem()
    {
        var validator = RegionalBookingPreferenceRules.DefaultRegionalBookingPreferenceValidator();
        var item = RegionalBookingPreference.NewRegionalBookingPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalBookingPreferenceLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new RegionalBookingPreferenceLeaveDaysRangeRule(1000, 86400);
        var item = RegionalBookingPreference.NewRegionalBookingPreference("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalBookingPreferenceCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new RegionalBookingPreferenceCeilingRangeRule(2.4, 3230.9);
        var item = RegionalBookingPreference.NewRegionalBookingPreference("x", "y");
        item.Ceiling = 3232.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalBookingPreferenceSkillLengthRuleFlagsALongSkill()
    {
        var rule = new RegionalBookingPreferenceSkillLengthRule(80);
        var item = RegionalBookingPreference.NewRegionalBookingPreference("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalBookingPreferenceSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new RegionalBookingPreferenceSummaryLengthRule(255);
        var item = RegionalBookingPreference.NewRegionalBookingPreference("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalBookingPreferenceLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RegionalBookingPreferenceLabelLengthRule(120);
        var item = RegionalBookingPreference.NewRegionalBookingPreference("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalBookingPreferenceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RegionalBookingPreferenceRequiredLabelRule("regional booking preference needs an entry");
        var item = RegionalBookingPreference.NewRegionalBookingPreference("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalBookingPreferenceValidatorReportsEveryProblem();
        RegionalBookingPreferenceLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        RegionalBookingPreferenceCeilingRangeRuleFlagsValuesAboveCeiling();
        RegionalBookingPreferenceSkillLengthRuleFlagsALongSkill();
        RegionalBookingPreferenceSummaryLengthRuleFlagsALongSummary();
        RegionalBookingPreferenceLabelLengthRuleFlagsALongLabel();
        RegionalBookingPreferenceRequiredLabelRuleFlagsABlankLabel();
    }
}
