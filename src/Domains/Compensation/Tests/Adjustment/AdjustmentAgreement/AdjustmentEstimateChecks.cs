namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

/// <summary>Checks of adjustment agreement code, run by the project's runner.</summary>
public static class AdjustmentEstimateChecks
{
    public static void AdjustmentEstimateStartsInRequested()
    {
        var item = AdjustmentEstimate.NewAdjustmentEstimate("code-726", "title-383");
        Check.Equal(item.Status, AdjustmentEstimateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentEstimateMovesToGranted()
    {
        var item = AdjustmentEstimate.NewAdjustmentEstimate("code-726", "title-383");
        try
        {
            item.MoveTo(AdjustmentEstimateStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentEstimateStatus.Granted, "status");
    }

    public static void AdjustmentEstimateRejectsMovesFromExpired()
    {
        var item = AdjustmentEstimate.NewAdjustmentEstimate("code-726", "title-383");
        item.Status = AdjustmentEstimateStatus.Expired;
        Check.Fails(() => item.MoveTo(AdjustmentEstimateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdjustmentEstimateStartsInRequested();
        AdjustmentEstimateMovesToGranted();
        AdjustmentEstimateRejectsMovesFromExpired();
    }
}
