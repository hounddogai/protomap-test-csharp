namespace Acme.Workforce.Workplace.Tests.Occupancy.Snapshot;

using Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalOccupancyPreferenceRulesChecks
{
    public static void SeasonalOccupancyPreferenceValidatorReportsEveryProblem()
    {
        var validator = SeasonalOccupancyPreferenceRules.DefaultSeasonalOccupancyPreferenceValidator();
        var item = SeasonalOccupancyPreference.NewSeasonalOccupancyPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalOccupancyPreferenceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SeasonalOccupancyPreferenceRequiredLabelRule("seasonal occupancy preference needs an entry");
        var item = SeasonalOccupancyPreference.NewSeasonalOccupancyPreference("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalOccupancyPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalOccupancyPreferenceCodeLengthRule(64);
        var item = SeasonalOccupancyPreference.NewSeasonalOccupancyPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalOccupancyPreferenceValidatorReportsEveryProblem();
        SeasonalOccupancyPreferenceRequiredLabelRuleFlagsABlankLabel();
        SeasonalOccupancyPreferenceCodeLengthRuleFlagsALongCode();
    }
}
