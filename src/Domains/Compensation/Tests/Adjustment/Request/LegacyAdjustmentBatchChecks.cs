namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyAdjustmentBatchChecks
{
    public static void LegacyAdjustmentBatchStartsInPlanned()
    {
        var item = LegacyAdjustmentBatch.NewLegacyAdjustmentBatch("code-375", "title-283");
        Check.Equal(item.Status, LegacyAdjustmentBatchStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyAdjustmentBatchMovesToScheduled()
    {
        var item = LegacyAdjustmentBatch.NewLegacyAdjustmentBatch("code-375", "title-283");
        try
        {
            item.MoveTo(LegacyAdjustmentBatchStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyAdjustmentBatchStatus.Scheduled, "status");
    }

    public static void LegacyAdjustmentBatchRejectsMovesFromCancelled()
    {
        var item = LegacyAdjustmentBatch.NewLegacyAdjustmentBatch("code-375", "title-283");
        item.Status = LegacyAdjustmentBatchStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LegacyAdjustmentBatchStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LegacyAdjustmentBatchStartsInPlanned();
        LegacyAdjustmentBatchMovesToScheduled();
        LegacyAdjustmentBatchRejectsMovesFromCancelled();
    }
}
