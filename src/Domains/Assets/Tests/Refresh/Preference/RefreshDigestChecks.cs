namespace Acme.Workforce.Assets.Tests.Refresh.Preference;

using Acme.Workforce.Assets.Refresh.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RefreshDigestChecks
{
    public static void RefreshDigestStartsInIdle()
    {
        var item = RefreshDigest.NewRefreshDigest("code-709", "title-906");
        Check.Equal(item.Status, RefreshDigestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshDigestMovesToStopped()
    {
        var item = RefreshDigest.NewRefreshDigest("code-709", "title-906");
        try
        {
            item.MoveTo(RefreshDigestStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshDigestStatus.Stopped, "status");
    }

    public static void RefreshDigestRejectsMovesFromStopped()
    {
        var item = RefreshDigest.NewRefreshDigest("code-709", "title-906");
        item.Status = RefreshDigestStatus.Stopped;
        Check.Fails(() => item.MoveTo(RefreshDigestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RefreshDigestStartsInIdle();
        RefreshDigestMovesToStopped();
        RefreshDigestRejectsMovesFromStopped();
    }
}
