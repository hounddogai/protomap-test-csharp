namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeskSegmentChecks
{
    public static void DeskSegmentStartsInProposed()
    {
        var item = DeskSegment.NewDeskSegment("code-842", "title-519");
        Check.Equal(item.Status, DeskSegmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskSegmentMovesToRejected()
    {
        var item = DeskSegment.NewDeskSegment("code-842", "title-519");
        try
        {
            item.MoveTo(DeskSegmentStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskSegmentStatus.Rejected, "status");
    }

    public static void DeskSegmentRejectsMovesFromWithdrawn()
    {
        var item = DeskSegment.NewDeskSegment("code-842", "title-519");
        item.Status = DeskSegmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DeskSegmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DeskSegmentStartsInProposed();
        DeskSegmentMovesToRejected();
        DeskSegmentRejectsMovesFromWithdrawn();
    }
}
