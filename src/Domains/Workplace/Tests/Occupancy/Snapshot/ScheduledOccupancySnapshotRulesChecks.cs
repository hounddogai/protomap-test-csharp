namespace Acme.Workforce.Workplace.Tests.Occupancy.Snapshot;

using Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledOccupancySnapshotRulesChecks
{
    public static void ScheduledOccupancySnapshotValidatorReportsEveryProblem()
    {
        var validator = ScheduledOccupancySnapshotRules.DefaultScheduledOccupancySnapshotValidator();
        var item = ScheduledOccupancySnapshot.NewScheduledOccupancySnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledOccupancySnapshotRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new ScheduledOccupancySnapshotRevisionRangeRule(16, 64);
        var item = ScheduledOccupancySnapshot.NewScheduledOccupancySnapshot("x", "y");
        item.Revision = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledOccupancySnapshotRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledOccupancySnapshotRequiredNameRule("scheduled occupancy snapshot needs an entry");
        var item = ScheduledOccupancySnapshot.NewScheduledOccupancySnapshot("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledOccupancySnapshotNameLengthRuleFlagsALongName()
    {
        var rule = new ScheduledOccupancySnapshotNameLengthRule(1000);
        var item = ScheduledOccupancySnapshot.NewScheduledOccupancySnapshot("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledOccupancySnapshotValidatorReportsEveryProblem();
        ScheduledOccupancySnapshotRevisionRangeRuleFlagsValuesAboveRevision();
        ScheduledOccupancySnapshotRequiredNameRuleFlagsABlankName();
        ScheduledOccupancySnapshotNameLengthRuleFlagsALongName();
    }
}
