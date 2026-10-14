namespace Acme.Workforce.Workplace.Tests.AccessBadge.Rollup;

using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccessBadgeRequestChecks
{
    public static void AccessBadgeRequestStartsInIdle()
    {
        var item = AccessBadgeRequest.NewAccessBadgeRequest("code-146", "title-57");
        Check.Equal(item.Status, AccessBadgeRequestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeRequestMovesToWarming()
    {
        var item = AccessBadgeRequest.NewAccessBadgeRequest("code-146", "title-57");
        try
        {
            item.MoveTo(AccessBadgeRequestStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeRequestStatus.Warming, "status");
    }

    public static void AccessBadgeRequestRejectsMovesFromStopped()
    {
        var item = AccessBadgeRequest.NewAccessBadgeRequest("code-146", "title-57");
        item.Status = AccessBadgeRequestStatus.Stopped;
        Check.Fails(() => item.MoveTo(AccessBadgeRequestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AccessBadgeRequestStartsInIdle();
        AccessBadgeRequestMovesToWarming();
        AccessBadgeRequestRejectsMovesFromStopped();
    }
}
