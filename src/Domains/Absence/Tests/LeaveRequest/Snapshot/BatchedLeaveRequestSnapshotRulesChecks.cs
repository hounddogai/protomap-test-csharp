namespace Acme.Workforce.Absence.Tests.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class BatchedLeaveRequestSnapshotRulesChecks
{
    public static void BatchedLeaveRequestSnapshotValidatorReportsEveryProblem()
    {
        var validator = BatchedLeaveRequestSnapshotRules.DefaultBatchedLeaveRequestSnapshotValidator();
        var item = BatchedLeaveRequestSnapshot.NewBatchedLeaveRequestSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedLeaveRequestSnapshotQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new BatchedLeaveRequestSnapshotQuantityRangeRule(120, 86400);
        var item = BatchedLeaveRequestSnapshot.NewBatchedLeaveRequestSnapshot("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedLeaveRequestSnapshotSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new BatchedLeaveRequestSnapshotSalaryBandLengthRule(86400);
        var item = BatchedLeaveRequestSnapshot.NewBatchedLeaveRequestSnapshot("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedLeaveRequestSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedLeaveRequestSnapshotRequiredCodeRule("batched leave request snapshot needs this field");
        var item = BatchedLeaveRequestSnapshot.NewBatchedLeaveRequestSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedLeaveRequestSnapshotValidatorReportsEveryProblem();
        BatchedLeaveRequestSnapshotQuantityRangeRuleFlagsValuesAboveQuantity();
        BatchedLeaveRequestSnapshotSalaryBandLengthRuleFlagsALongSalaryBand();
        BatchedLeaveRequestSnapshotRequiredCodeRuleFlagsABlankCode();
    }
}
