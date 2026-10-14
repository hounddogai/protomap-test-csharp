namespace Acme.Workforce.Compensation.Tests.Adjustment.Estimate;

using Acme.Workforce.Compensation.Adjustment.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PendingAdjustmentEstimateChecks
{
    public static void PendingAdjustmentEstimateStartsInRequested()
    {
        var item = PendingAdjustmentEstimate.NewPendingAdjustmentEstimate("code-585", "label-17");
        Check.Equal(item.Status, PendingAdjustmentEstimateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingAdjustmentEstimateMovesToRevoked()
    {
        var item = PendingAdjustmentEstimate.NewPendingAdjustmentEstimate("code-585", "label-17");
        try
        {
            item.MoveTo(PendingAdjustmentEstimateStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingAdjustmentEstimateStatus.Revoked, "status");
    }

    public static void PendingAdjustmentEstimateRejectsMovesFromExpired()
    {
        var item = PendingAdjustmentEstimate.NewPendingAdjustmentEstimate("code-585", "label-17");
        item.Status = PendingAdjustmentEstimateStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingAdjustmentEstimateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingAdjustmentEstimateStartsInRequested();
        PendingAdjustmentEstimateMovesToRevoked();
        PendingAdjustmentEstimateRejectsMovesFromExpired();
    }
}
