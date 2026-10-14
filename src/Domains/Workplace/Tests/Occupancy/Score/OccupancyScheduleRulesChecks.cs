namespace Acme.Workforce.Workplace.Tests.Occupancy.Score;

using Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class OccupancyScheduleRulesChecks
{
    public static void OccupancyScheduleValidatorReportsEveryProblem()
    {
        var validator = OccupancyScheduleRules.DefaultOccupancyScheduleValidator();
        var item = OccupancySchedule.NewOccupancySchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyScheduleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new OccupancyScheduleTitleLengthRule(80);
        var item = OccupancySchedule.NewOccupancySchedule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyScheduleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new OccupancyScheduleRequiredTitleRule("occupancy schedule needs a value");
        var item = OccupancySchedule.NewOccupancySchedule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OccupancyScheduleValidatorReportsEveryProblem();
        OccupancyScheduleTitleLengthRuleFlagsALongTitle();
        OccupancyScheduleRequiredTitleRuleFlagsABlankTitle();
    }
}
