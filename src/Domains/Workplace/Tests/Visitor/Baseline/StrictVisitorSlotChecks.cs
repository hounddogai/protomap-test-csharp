namespace Acme.Workforce.Workplace.Tests.Visitor.Baseline;

using Acme.Workforce.Workplace.Visitor.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class StrictVisitorSlotChecks
{
    public static void StrictVisitorSlotStartsInOpen()
    {
        var item = StrictVisitorSlot.NewStrictVisitorSlot("code-425", "name-57");
        Check.Equal(item.Status, StrictVisitorSlotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictVisitorSlotMovesToInReview()
    {
        var item = StrictVisitorSlot.NewStrictVisitorSlot("code-425", "name-57");
        try
        {
            item.MoveTo(StrictVisitorSlotStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictVisitorSlotStatus.InReview, "status");
    }

    public static void StrictVisitorSlotRejectsMovesFromArchived()
    {
        var item = StrictVisitorSlot.NewStrictVisitorSlot("code-425", "name-57");
        item.Status = StrictVisitorSlotStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictVisitorSlotStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictVisitorSlotStartsInOpen();
        StrictVisitorSlotMovesToInReview();
        StrictVisitorSlotRejectsMovesFromArchived();
    }
}
