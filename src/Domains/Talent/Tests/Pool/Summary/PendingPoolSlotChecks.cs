namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingPoolSlotChecks
{
    public static void PendingPoolSlotStartsInIdle()
    {
        var item = PendingPoolSlot.NewPendingPoolSlot("code-917", "label-91");
        Check.Equal(item.Status, PendingPoolSlotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingPoolSlotMovesToWarming()
    {
        var item = PendingPoolSlot.NewPendingPoolSlot("code-917", "label-91");
        try
        {
            item.MoveTo(PendingPoolSlotStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingPoolSlotStatus.Warming, "status");
    }

    public static void PendingPoolSlotRejectsMovesFromStopped()
    {
        var item = PendingPoolSlot.NewPendingPoolSlot("code-917", "label-91");
        item.Status = PendingPoolSlotStatus.Stopped;
        Check.Fails(() => item.MoveTo(PendingPoolSlotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PendingPoolSlotStartsInIdle();
        PendingPoolSlotMovesToWarming();
        PendingPoolSlotRejectsMovesFromStopped();
    }
}
