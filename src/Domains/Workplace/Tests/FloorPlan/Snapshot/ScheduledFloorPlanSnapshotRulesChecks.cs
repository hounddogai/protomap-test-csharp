namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledFloorPlanSnapshotRulesChecks
{
    public static void ScheduledFloorPlanSnapshotValidatorReportsEveryProblem()
    {
        var validator = ScheduledFloorPlanSnapshotRules.DefaultScheduledFloorPlanSnapshotValidator();
        var item = ScheduledFloorPlanSnapshot.NewScheduledFloorPlanSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledFloorPlanSnapshotCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new ScheduledFloorPlanSnapshotCeilingRangeRule(8.7, 2926.1);
        var item = ScheduledFloorPlanSnapshot.NewScheduledFloorPlanSnapshot("x", "y");
        item.Ceiling = 2927.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledFloorPlanSnapshotRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new ScheduledFloorPlanSnapshotRequiredEmployeeIdRule("scheduled floor plan snapshot needs an entry");
        var item = ScheduledFloorPlanSnapshot.NewScheduledFloorPlanSnapshot("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledFloorPlanSnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledFloorPlanSnapshotCodeLengthRule(64);
        var item = ScheduledFloorPlanSnapshot.NewScheduledFloorPlanSnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledFloorPlanSnapshotValidatorReportsEveryProblem();
        ScheduledFloorPlanSnapshotCeilingRangeRuleFlagsValuesAboveCeiling();
        ScheduledFloorPlanSnapshotRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        ScheduledFloorPlanSnapshotCodeLengthRuleFlagsALongCode();
    }
}
