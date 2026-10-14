namespace Acme.Workforce.Workplace.Tests.AccessBadge.Target;

using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AccessBadgeSummaryChecks
{
    public static void AccessBadgeSummaryStartsInIdle()
    {
        var item = AccessBadgeSummary.NewAccessBadgeSummary("code-592", "name-680");
        Check.Equal(item.Status, AccessBadgeSummaryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeSummaryMovesToStopped()
    {
        var item = AccessBadgeSummary.NewAccessBadgeSummary("code-592", "name-680");
        try
        {
            item.MoveTo(AccessBadgeSummaryStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeSummaryStatus.Stopped, "status");
    }

    public static void AccessBadgeSummaryRejectsMovesFromStopped()
    {
        var item = AccessBadgeSummary.NewAccessBadgeSummary("code-592", "name-680");
        item.Status = AccessBadgeSummaryStatus.Stopped;
        Check.Fails(() => item.MoveTo(AccessBadgeSummaryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AccessBadgeSummaryStartsInIdle();
        AccessBadgeSummaryMovesToStopped();
        AccessBadgeSummaryRejectsMovesFromStopped();
    }
}
