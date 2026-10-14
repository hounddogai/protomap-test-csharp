namespace Acme.Workforce.Workplace.Tests.Desk.Slot;

using Acme.Workforce.Workplace.Desk.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeskSlotChecks
{
    public static void DeskSlotStartsInIdle()
    {
        var item = DeskSlot.NewDeskSlot("code-151", "label-66");
        Check.Equal(item.Status, DeskSlotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskSlotMovesToWarming()
    {
        var item = DeskSlot.NewDeskSlot("code-151", "label-66");
        try
        {
            item.MoveTo(DeskSlotStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskSlotStatus.Warming, "status");
    }

    public static void DeskSlotRejectsMovesFromStopped()
    {
        var item = DeskSlot.NewDeskSlot("code-151", "label-66");
        item.Status = DeskSlotStatus.Stopped;
        Check.Fails(() => item.MoveTo(DeskSlotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DeskSlotStartsInIdle();
        DeskSlotMovesToWarming();
        DeskSlotRejectsMovesFromStopped();
    }
}
