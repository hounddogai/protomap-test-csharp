namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class ScheduledAdjustmentRequestChecks
{
    public static void ScheduledAdjustmentRequestStartsInOpen()
    {
        var item = ScheduledAdjustmentRequest.NewScheduledAdjustmentRequest("code-312", "label-378");
        Check.Equal(item.Status, ScheduledAdjustmentRequestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledAdjustmentRequestMovesToResolved()
    {
        var item = ScheduledAdjustmentRequest.NewScheduledAdjustmentRequest("code-312", "label-378");
        try
        {
            item.MoveTo(ScheduledAdjustmentRequestStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledAdjustmentRequestStatus.Resolved, "status");
    }

    public static void ScheduledAdjustmentRequestRejectsMovesFromArchived()
    {
        var item = ScheduledAdjustmentRequest.NewScheduledAdjustmentRequest("code-312", "label-378");
        item.Status = ScheduledAdjustmentRequestStatus.Archived;
        Check.Fails(() => item.MoveTo(ScheduledAdjustmentRequestStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScheduledAdjustmentRequestStartsInOpen();
        ScheduledAdjustmentRequestMovesToResolved();
        ScheduledAdjustmentRequestRejectsMovesFromArchived();
    }
}
