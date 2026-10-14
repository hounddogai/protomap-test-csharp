namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleSlotChecks
{
    public static void MeritCycleSlotStartsInDraft()
    {
        var item = MeritCycleSlot.NewMeritCycleSlot("code-162", "title-761");
        Check.Equal(item.Status, MeritCycleSlotStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleSlotMovesToPending()
    {
        var item = MeritCycleSlot.NewMeritCycleSlot("code-162", "title-761");
        try
        {
            item.MoveTo(MeritCycleSlotStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleSlotStatus.Pending, "status");
    }

    public static void MeritCycleSlotRejectsMovesFromClosed()
    {
        var item = MeritCycleSlot.NewMeritCycleSlot("code-162", "title-761");
        item.Status = MeritCycleSlotStatus.Closed;
        Check.Fails(() => item.MoveTo(MeritCycleSlotStatus.Draft), "move");
    }

    public static void RunAll()
    {
        MeritCycleSlotStartsInDraft();
        MeritCycleSlotMovesToPending();
        MeritCycleSlotRejectsMovesFromClosed();
    }
}
