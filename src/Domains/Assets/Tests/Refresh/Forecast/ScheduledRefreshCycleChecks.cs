namespace Acme.Workforce.Assets.Tests.Refresh.Forecast;

using Acme.Workforce.Assets.Refresh.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class ScheduledRefreshCycleChecks
{
    public static void ScheduledRefreshCycleStartsInCreated()
    {
        var item = ScheduledRefreshCycle.NewScheduledRefreshCycle("code-306", "label-989");
        Check.Equal(item.Status, ScheduledRefreshCycleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledRefreshCycleMovesToPublished()
    {
        var item = ScheduledRefreshCycle.NewScheduledRefreshCycle("code-306", "label-989");
        try
        {
            item.MoveTo(ScheduledRefreshCycleStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledRefreshCycleStatus.Published, "status");
    }

    public static void ScheduledRefreshCycleRejectsMovesFromRetired()
    {
        var item = ScheduledRefreshCycle.NewScheduledRefreshCycle("code-306", "label-989");
        item.Status = ScheduledRefreshCycleStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledRefreshCycleStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledRefreshCycleStartsInCreated();
        ScheduledRefreshCycleMovesToPublished();
        ScheduledRefreshCycleRejectsMovesFromRetired();
    }
}
