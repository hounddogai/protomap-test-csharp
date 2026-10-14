namespace Acme.Workforce.Workplace.Tests.AccessBadge.Audit;

using Acme.Workforce.Workplace.AccessBadge.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AccessBadgeThresholdChecks
{
    public static void AccessBadgeThresholdStartsInIdle()
    {
        var item = AccessBadgeThreshold.NewAccessBadgeThreshold("code-498", "name-5");
        Check.Equal(item.Status, AccessBadgeThresholdStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeThresholdMovesToWarming()
    {
        var item = AccessBadgeThreshold.NewAccessBadgeThreshold("code-498", "name-5");
        try
        {
            item.MoveTo(AccessBadgeThresholdStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeThresholdStatus.Warming, "status");
    }

    public static void AccessBadgeThresholdRejectsMovesFromStopped()
    {
        var item = AccessBadgeThreshold.NewAccessBadgeThreshold("code-498", "name-5");
        item.Status = AccessBadgeThresholdStatus.Stopped;
        Check.Fails(() => item.MoveTo(AccessBadgeThresholdStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AccessBadgeThresholdStartsInIdle();
        AccessBadgeThresholdMovesToWarming();
        AccessBadgeThresholdRejectsMovesFromStopped();
    }
}
