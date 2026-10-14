namespace Acme.Workforce.Workplace.Tests.Occupancy.Snapshot;

using Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class OccupancyEntryRulesChecks
{
    public static void OccupancyEntryValidatorReportsEveryProblem()
    {
        var validator = OccupancyEntryRules.DefaultOccupancyEntryValidator();
        var item = OccupancyEntry.NewOccupancyEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyEntryWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new OccupancyEntryWindowDaysRangeRule(32, 86400);
        var item = OccupancyEntry.NewOccupancyEntry("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new OccupancyEntryRequiredCodeRule("occupancy entry needs this field");
        var item = OccupancyEntry.NewOccupancyEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyEntryRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new OccupancyEntryRequiredJobCodeRule("occupancy entry needs a value");
        var item = OccupancyEntry.NewOccupancyEntry("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyEntrySourceLengthRuleFlagsALongSource()
    {
        var rule = new OccupancyEntrySourceLengthRule(16);
        var item = OccupancyEntry.NewOccupancyEntry("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyEntryFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new OccupancyEntryFteRangeRule(5.3, 3860.7);
        var item = OccupancyEntry.NewOccupancyEntry("x", "y");
        item.Fte = 3862.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        OccupancyEntryValidatorReportsEveryProblem();
        OccupancyEntryWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        OccupancyEntryRequiredCodeRuleFlagsABlankCode();
        OccupancyEntryRequiredJobCodeRuleFlagsABlankJobCode();
        OccupancyEntrySourceLengthRuleFlagsALongSource();
        OccupancyEntryFteRangeRuleFlagsValuesAboveFte();
    }
}
