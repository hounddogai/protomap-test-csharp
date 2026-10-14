namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshRequestChecks
{
    public static void RefreshRequestStartsInProposed()
    {
        var item = RefreshRequest.NewRefreshRequest("code-482", "title-631");
        Check.Equal(item.Status, RefreshRequestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshRequestMovesToApproved()
    {
        var item = RefreshRequest.NewRefreshRequest("code-482", "title-631");
        try
        {
            item.MoveTo(RefreshRequestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshRequestStatus.Approved, "status");
    }

    public static void RefreshRequestRejectsMovesFromWithdrawn()
    {
        var item = RefreshRequest.NewRefreshRequest("code-482", "title-631");
        item.Status = RefreshRequestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RefreshRequestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RefreshRequestStartsInProposed();
        RefreshRequestMovesToApproved();
        RefreshRequestRejectsMovesFromWithdrawn();
    }
}
