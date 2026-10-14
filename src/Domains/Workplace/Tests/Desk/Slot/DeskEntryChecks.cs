namespace Acme.Workforce.Workplace.Tests.Desk.Slot;

using Acme.Workforce.Workplace.Desk.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeskEntryChecks
{
    public static void DeskEntryStartsInIdle()
    {
        var item = DeskEntry.NewDeskEntry("code-974", "label-838");
        Check.Equal(item.Status, DeskEntryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskEntryMovesToWarming()
    {
        var item = DeskEntry.NewDeskEntry("code-974", "label-838");
        try
        {
            item.MoveTo(DeskEntryStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskEntryStatus.Warming, "status");
    }

    public static void DeskEntryRejectsMovesFromStopped()
    {
        var item = DeskEntry.NewDeskEntry("code-974", "label-838");
        item.Status = DeskEntryStatus.Stopped;
        Check.Fails(() => item.MoveTo(DeskEntryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DeskEntryStartsInIdle();
        DeskEntryMovesToWarming();
        DeskEntryRejectsMovesFromStopped();
    }
}
