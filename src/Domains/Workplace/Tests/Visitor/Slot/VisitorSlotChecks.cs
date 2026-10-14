namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorSlotChecks
{
    public static void VisitorSlotStartsInNew()
    {
        var item = VisitorSlot.NewVisitorSlot("code-465", "label-865");
        Check.Equal(item.Status, VisitorSlotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorSlotMovesToRunning()
    {
        var item = VisitorSlot.NewVisitorSlot("code-465", "label-865");
        try
        {
            item.MoveTo(VisitorSlotStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorSlotStatus.Running, "status");
    }

    public static void VisitorSlotRejectsMovesFromFailed()
    {
        var item = VisitorSlot.NewVisitorSlot("code-465", "label-865");
        item.Status = VisitorSlotStatus.Failed;
        Check.Fails(() => item.MoveTo(VisitorSlotStatus.New), "move");
    }

    public static void RunAll()
    {
        VisitorSlotStartsInNew();
        VisitorSlotMovesToRunning();
        VisitorSlotRejectsMovesFromFailed();
    }
}
