namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class PendingVisitorSlotChecks
{
    public static void PendingVisitorSlotStartsInOpen()
    {
        var item = PendingVisitorSlot.NewPendingVisitorSlot("code-319", "title-722");
        Check.Equal(item.Status, PendingVisitorSlotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingVisitorSlotMovesToArchived()
    {
        var item = PendingVisitorSlot.NewPendingVisitorSlot("code-319", "title-722");
        try
        {
            item.MoveTo(PendingVisitorSlotStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingVisitorSlotStatus.Archived, "status");
    }

    public static void PendingVisitorSlotRejectsMovesFromArchived()
    {
        var item = PendingVisitorSlot.NewPendingVisitorSlot("code-319", "title-722");
        item.Status = PendingVisitorSlotStatus.Archived;
        Check.Fails(() => item.MoveTo(PendingVisitorSlotStatus.Open), "move");
    }

    public static void RunAll()
    {
        PendingVisitorSlotStartsInOpen();
        PendingVisitorSlotMovesToArchived();
        PendingVisitorSlotRejectsMovesFromArchived();
    }
}
