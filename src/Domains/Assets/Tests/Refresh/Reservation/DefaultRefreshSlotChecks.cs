namespace Acme.Workforce.Assets.Tests.Refresh.Reservation;

using Acme.Workforce.Assets.Refresh.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class DefaultRefreshSlotChecks
{
    public static void DefaultRefreshSlotStartsInIdle()
    {
        var item = DefaultRefreshSlot.NewDefaultRefreshSlot("code-613", "name-855");
        Check.Equal(item.Status, DefaultRefreshSlotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultRefreshSlotMovesToWarming()
    {
        var item = DefaultRefreshSlot.NewDefaultRefreshSlot("code-613", "name-855");
        try
        {
            item.MoveTo(DefaultRefreshSlotStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultRefreshSlotStatus.Warming, "status");
    }

    public static void DefaultRefreshSlotRejectsMovesFromStopped()
    {
        var item = DefaultRefreshSlot.NewDefaultRefreshSlot("code-613", "name-855");
        item.Status = DefaultRefreshSlotStatus.Stopped;
        Check.Fails(() => item.MoveTo(DefaultRefreshSlotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DefaultRefreshSlotStartsInIdle();
        DefaultRefreshSlotMovesToWarming();
        DefaultRefreshSlotRejectsMovesFromStopped();
    }
}
