namespace Acme.Workforce.Workplace.Tests.Occupancy.Draft;

using Acme.Workforce.Workplace.Occupancy.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class OccupancyDraftChecks
{
    public static void OccupancyDraftStartsInProposed()
    {
        var item = OccupancyDraft.NewOccupancyDraft("code-831", "label-508");
        Check.Equal(item.Status, OccupancyDraftStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyDraftMovesToRejected()
    {
        var item = OccupancyDraft.NewOccupancyDraft("code-831", "label-508");
        try
        {
            item.MoveTo(OccupancyDraftStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyDraftStatus.Rejected, "status");
    }

    public static void OccupancyDraftRejectsMovesFromWithdrawn()
    {
        var item = OccupancyDraft.NewOccupancyDraft("code-831", "label-508");
        item.Status = OccupancyDraftStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(OccupancyDraftStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        OccupancyDraftStartsInProposed();
        OccupancyDraftMovesToRejected();
        OccupancyDraftRejectsMovesFromWithdrawn();
    }
}
