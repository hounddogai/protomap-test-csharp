namespace Acme.Workforce.Compensation.Tests.Adjustment.Agreement;

using Acme.Workforce.Compensation.Adjustment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AdjustmentRequestChecks
{
    public static void AdjustmentRequestStartsInRequested()
    {
        var item = AdjustmentRequest.NewAdjustmentRequest("code-697", "label-253");
        Check.Equal(item.Status, AdjustmentRequestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentRequestMovesToGranted()
    {
        var item = AdjustmentRequest.NewAdjustmentRequest("code-697", "label-253");
        try
        {
            item.MoveTo(AdjustmentRequestStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentRequestStatus.Granted, "status");
    }

    public static void AdjustmentRequestRejectsMovesFromExpired()
    {
        var item = AdjustmentRequest.NewAdjustmentRequest("code-697", "label-253");
        item.Status = AdjustmentRequestStatus.Expired;
        Check.Fails(() => item.MoveTo(AdjustmentRequestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdjustmentRequestStartsInRequested();
        AdjustmentRequestMovesToGranted();
        AdjustmentRequestRejectsMovesFromExpired();
    }
}
