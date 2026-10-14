namespace Acme.Workforce.Assets.Tests.Refresh.Estimate;

using Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryRefreshEstimateChecks
{
    public static void PrimaryRefreshEstimateStartsInRequested()
    {
        var item = PrimaryRefreshEstimate.NewPrimaryRefreshEstimate("code-525", "label-623");
        Check.Equal(item.Status, PrimaryRefreshEstimateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryRefreshEstimateMovesToRevoked()
    {
        var item = PrimaryRefreshEstimate.NewPrimaryRefreshEstimate("code-525", "label-623");
        try
        {
            item.MoveTo(PrimaryRefreshEstimateStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryRefreshEstimateStatus.Revoked, "status");
    }

    public static void PrimaryRefreshEstimateRejectsMovesFromExpired()
    {
        var item = PrimaryRefreshEstimate.NewPrimaryRefreshEstimate("code-525", "label-623");
        item.Status = PrimaryRefreshEstimateStatus.Expired;
        Check.Fails(() => item.MoveTo(PrimaryRefreshEstimateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PrimaryRefreshEstimateStartsInRequested();
        PrimaryRefreshEstimateMovesToRevoked();
        PrimaryRefreshEstimateRejectsMovesFromExpired();
    }
}
