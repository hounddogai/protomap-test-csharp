namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeRequest;

using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Checks of badge request code, run by the project's runner.</summary>
public static class RollingAccessBadgeRequestChecks
{
    public static void RollingAccessBadgeRequestStartsInIdle()
    {
        var item = RollingAccessBadgeRequest.NewRollingAccessBadgeRequest("code-645", "label-901");
        Check.Equal(item.Status, RollingAccessBadgeRequestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingAccessBadgeRequestMovesToWarming()
    {
        var item = RollingAccessBadgeRequest.NewRollingAccessBadgeRequest("code-645", "label-901");
        try
        {
            item.MoveTo(RollingAccessBadgeRequestStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingAccessBadgeRequestStatus.Warming, "status");
    }

    public static void RollingAccessBadgeRequestRejectsMovesFromStopped()
    {
        var item = RollingAccessBadgeRequest.NewRollingAccessBadgeRequest("code-645", "label-901");
        item.Status = RollingAccessBadgeRequestStatus.Stopped;
        Check.Fails(() => item.MoveTo(RollingAccessBadgeRequestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RollingAccessBadgeRequestStartsInIdle();
        RollingAccessBadgeRequestMovesToWarming();
        RollingAccessBadgeRequestRejectsMovesFromStopped();
    }
}
