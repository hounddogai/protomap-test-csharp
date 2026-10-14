namespace Acme.Workforce.Compensation.Tests.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AdjustmentClaimChecks
{
    public static void AdjustmentClaimStartsInPlanned()
    {
        var item = AdjustmentClaim.NewAdjustmentClaim("code-609", "label-109");
        Check.Equal(item.Status, AdjustmentClaimStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentClaimMovesToScheduled()
    {
        var item = AdjustmentClaim.NewAdjustmentClaim("code-609", "label-109");
        try
        {
            item.MoveTo(AdjustmentClaimStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentClaimStatus.Scheduled, "status");
    }

    public static void AdjustmentClaimRejectsMovesFromCancelled()
    {
        var item = AdjustmentClaim.NewAdjustmentClaim("code-609", "label-109");
        item.Status = AdjustmentClaimStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdjustmentClaimStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdjustmentClaimStartsInPlanned();
        AdjustmentClaimMovesToScheduled();
        AdjustmentClaimRejectsMovesFromCancelled();
    }
}
