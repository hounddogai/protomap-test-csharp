namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RegionalRefreshRequestChecks
{
    public static void RegionalRefreshRequestStartsInDraft()
    {
        var item = RegionalRefreshRequest.NewRegionalRefreshRequest("code-620", "label-996");
        Check.Equal(item.Status, RegionalRefreshRequestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalRefreshRequestMovesToSuspended()
    {
        var item = RegionalRefreshRequest.NewRegionalRefreshRequest("code-620", "label-996");
        try
        {
            item.MoveTo(RegionalRefreshRequestStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalRefreshRequestStatus.Suspended, "status");
    }

    public static void RegionalRefreshRequestRejectsMovesFromClosed()
    {
        var item = RegionalRefreshRequest.NewRegionalRefreshRequest("code-620", "label-996");
        item.Status = RegionalRefreshRequestStatus.Closed;
        Check.Fails(() => item.MoveTo(RegionalRefreshRequestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RegionalRefreshRequestStartsInDraft();
        RegionalRefreshRequestMovesToSuspended();
        RegionalRefreshRequestRejectsMovesFromClosed();
    }
}
