namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class ScheduledPoolWindowChecks
{
    public static void ScheduledPoolWindowStartsInRequested()
    {
        var item = ScheduledPoolWindow.NewScheduledPoolWindow("code-334", "title-570");
        Check.Equal(item.Status, ScheduledPoolWindowStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPoolWindowMovesToRevoked()
    {
        var item = ScheduledPoolWindow.NewScheduledPoolWindow("code-334", "title-570");
        try
        {
            item.MoveTo(ScheduledPoolWindowStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPoolWindowStatus.Revoked, "status");
    }

    public static void ScheduledPoolWindowRejectsMovesFromExpired()
    {
        var item = ScheduledPoolWindow.NewScheduledPoolWindow("code-334", "title-570");
        item.Status = ScheduledPoolWindowStatus.Expired;
        Check.Fails(() => item.MoveTo(ScheduledPoolWindowStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ScheduledPoolWindowStartsInRequested();
        ScheduledPoolWindowMovesToRevoked();
        ScheduledPoolWindowRejectsMovesFromExpired();
    }
}
