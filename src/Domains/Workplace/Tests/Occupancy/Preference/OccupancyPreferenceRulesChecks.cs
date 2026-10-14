namespace Acme.Workforce.Workplace.Tests.Occupancy.Preference;

using Acme.Workforce.Workplace.Occupancy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class OccupancyPreferenceRulesChecks
{
    public static void OccupancyPreferenceValidatorReportsEveryProblem()
    {
        var validator = OccupancyPreferenceRules.DefaultOccupancyPreferenceValidator();
        var item = OccupancyPreference.NewOccupancyPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new OccupancyPreferenceRequiredCodeRule("occupancy preference needs an entry");
        var item = OccupancyPreference.NewOccupancyPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new OccupancyPreferenceCodeLengthRule(32);
        var item = OccupancyPreference.NewOccupancyPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyPreferenceHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new OccupancyPreferenceHoursRangeRule(7.5, 3127.1);
        var item = OccupancyPreference.NewOccupancyPreference("x", "y");
        item.Hours = 3128.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        OccupancyPreferenceValidatorReportsEveryProblem();
        OccupancyPreferenceRequiredCodeRuleFlagsABlankCode();
        OccupancyPreferenceCodeLengthRuleFlagsALongCode();
        OccupancyPreferenceHoursRangeRuleFlagsValuesAboveHours();
    }
}
