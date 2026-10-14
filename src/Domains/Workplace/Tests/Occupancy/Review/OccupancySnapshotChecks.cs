namespace Acme.Workforce.Workplace.Tests.Occupancy.Review;

using Acme.Workforce.Workplace.Occupancy.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class OccupancySnapshotChecks
{
    public static void OccupancySnapshotStartsInOpen()
    {
        var item = OccupancySnapshot.NewOccupancySnapshot("code-289", "title-96");
        Check.Equal(item.Status, OccupancySnapshotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancySnapshotMovesToInReview()
    {
        var item = OccupancySnapshot.NewOccupancySnapshot("code-289", "title-96");
        try
        {
            item.MoveTo(OccupancySnapshotStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancySnapshotStatus.InReview, "status");
    }

    public static void OccupancySnapshotRejectsMovesFromArchived()
    {
        var item = OccupancySnapshot.NewOccupancySnapshot("code-289", "title-96");
        item.Status = OccupancySnapshotStatus.Archived;
        Check.Fails(() => item.MoveTo(OccupancySnapshotStatus.Open), "move");
    }

    public static void RunAll()
    {
        OccupancySnapshotStartsInOpen();
        OccupancySnapshotMovesToInReview();
        OccupancySnapshotRejectsMovesFromArchived();
    }
}
