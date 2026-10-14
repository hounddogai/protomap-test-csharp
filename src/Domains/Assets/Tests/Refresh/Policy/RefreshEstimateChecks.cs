namespace Acme.Workforce.Assets.Tests.Refresh.Policy;

using Acme.Workforce.Assets.Refresh.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RefreshEstimateChecks
{
    public static void RefreshEstimateStartsInRequested()
    {
        var item = RefreshEstimate.NewRefreshEstimate("code-367", "name-361");
        Check.Equal(item.Status, RefreshEstimateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshEstimateMovesToRevoked()
    {
        var item = RefreshEstimate.NewRefreshEstimate("code-367", "name-361");
        try
        {
            item.MoveTo(RefreshEstimateStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshEstimateStatus.Revoked, "status");
    }

    public static void RefreshEstimateRejectsMovesFromExpired()
    {
        var item = RefreshEstimate.NewRefreshEstimate("code-367", "name-361");
        item.Status = RefreshEstimateStatus.Expired;
        Check.Fails(() => item.MoveTo(RefreshEstimateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RefreshEstimateStartsInRequested();
        RefreshEstimateMovesToRevoked();
        RefreshEstimateRejectsMovesFromExpired();
    }
}
