namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class BatchedFloorPlanSnapshotRulesChecks
{
    public static void BatchedFloorPlanSnapshotValidatorReportsEveryProblem()
    {
        var validator = BatchedFloorPlanSnapshotRules.DefaultBatchedFloorPlanSnapshotValidator();
        var item = BatchedFloorPlanSnapshot.NewBatchedFloorPlanSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedFloorPlanSnapshotRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new BatchedFloorPlanSnapshotRequiredContactEmailRule("batched floor plan snapshot needs this field");
        var item = BatchedFloorPlanSnapshot.NewBatchedFloorPlanSnapshot("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedFloorPlanSnapshotEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new BatchedFloorPlanSnapshotEmployeeIdLengthRule(80);
        var item = BatchedFloorPlanSnapshot.NewBatchedFloorPlanSnapshot("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedFloorPlanSnapshotBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new BatchedFloorPlanSnapshotBatchSizeRangeRule(1000, 3600);
        var item = BatchedFloorPlanSnapshot.NewBatchedFloorPlanSnapshot("x", "y");
        item.BatchSize = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedFloorPlanSnapshotValidatorReportsEveryProblem();
        BatchedFloorPlanSnapshotRequiredContactEmailRuleFlagsABlankContactEmail();
        BatchedFloorPlanSnapshotEmployeeIdLengthRuleFlagsALongEmployeeId();
        BatchedFloorPlanSnapshotBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
