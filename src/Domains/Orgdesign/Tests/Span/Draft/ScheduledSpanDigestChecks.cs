namespace Acme.Workforce.Orgdesign.Tests.Span.Draft;

using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledSpanDigestChecks
{
    public static void ScheduledSpanDigestStartsInIdle()
    {
        var item = ScheduledSpanDigest.NewScheduledSpanDigest("code-664", "label-658");
        Check.Equal(item.Status, ScheduledSpanDigestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledSpanDigestMovesToReady()
    {
        var item = ScheduledSpanDigest.NewScheduledSpanDigest("code-664", "label-658");
        try
        {
            item.MoveTo(ScheduledSpanDigestStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledSpanDigestStatus.Ready, "status");
    }

    public static void ScheduledSpanDigestRejectsMovesFromStopped()
    {
        var item = ScheduledSpanDigest.NewScheduledSpanDigest("code-664", "label-658");
        item.Status = ScheduledSpanDigestStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScheduledSpanDigestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScheduledSpanDigestStartsInIdle();
        ScheduledSpanDigestMovesToReady();
        ScheduledSpanDigestRejectsMovesFromStopped();
    }
}
