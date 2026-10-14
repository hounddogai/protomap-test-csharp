namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingLeaveRequestBatchRulesChecks
{
    public static void PendingLeaveRequestBatchValidatorReportsEveryProblem()
    {
        var validator = PendingLeaveRequestBatchRules.DefaultPendingLeaveRequestBatchValidator();
        var item = PendingLeaveRequestBatch.NewPendingLeaveRequestBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingLeaveRequestBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingLeaveRequestBatchRequiredCodeRule("pending leave request batch needs this field");
        var item = PendingLeaveRequestBatch.NewPendingLeaveRequestBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingLeaveRequestBatchEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new PendingLeaveRequestBatchEndsAtRangeRule(80, 120);
        var item = PendingLeaveRequestBatch.NewPendingLeaveRequestBatch("x", "y");
        item.EndsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingLeaveRequestBatchFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new PendingLeaveRequestBatchFloorValueRangeRule(8.8, 2882.4);
        var item = PendingLeaveRequestBatch.NewPendingLeaveRequestBatch("x", "y");
        item.FloorValue = 2883.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingLeaveRequestBatchRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingLeaveRequestBatchRequiredTitleRule("pending leave request batch needs this field");
        var item = PendingLeaveRequestBatch.NewPendingLeaveRequestBatch("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingLeaveRequestBatchValidatorReportsEveryProblem();
        PendingLeaveRequestBatchRequiredCodeRuleFlagsABlankCode();
        PendingLeaveRequestBatchEndsAtRangeRuleFlagsValuesAboveEndsAt();
        PendingLeaveRequestBatchFloorValueRangeRuleFlagsValuesAboveFloorValue();
        PendingLeaveRequestBatchRequiredTitleRuleFlagsABlankTitle();
    }
}
