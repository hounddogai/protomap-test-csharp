namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshRollupChecks
{
    public static void RefreshRollupStartsInProposed()
    {
        var item = RefreshRollup.NewRefreshRollup("code-161", "label-754");
        Check.Equal(item.Status, RefreshRollupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshRollupMovesToRejected()
    {
        var item = RefreshRollup.NewRefreshRollup("code-161", "label-754");
        try
        {
            item.MoveTo(RefreshRollupStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshRollupStatus.Rejected, "status");
    }

    public static void RefreshRollupRejectsMovesFromWithdrawn()
    {
        var item = RefreshRollup.NewRefreshRollup("code-161", "label-754");
        item.Status = RefreshRollupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RefreshRollupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RefreshRollupStartsInProposed();
        RefreshRollupMovesToRejected();
        RefreshRollupRejectsMovesFromWithdrawn();
    }
}
