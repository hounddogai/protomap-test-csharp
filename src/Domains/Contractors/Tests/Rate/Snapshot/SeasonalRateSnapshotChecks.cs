namespace Acme.Workforce.Contractors.Tests.Rate.Snapshot;

using Acme.Workforce.Contractors.Rate.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalRateSnapshotChecks
{
    public static void SeasonalRateSnapshotStartsInCreated()
    {
        var item = SeasonalRateSnapshot.NewSeasonalRateSnapshot("code-932", "label-16");
        Check.Equal(item.Status, SeasonalRateSnapshotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalRateSnapshotMovesToPublished()
    {
        var item = SeasonalRateSnapshot.NewSeasonalRateSnapshot("code-932", "label-16");
        try
        {
            item.MoveTo(SeasonalRateSnapshotStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalRateSnapshotStatus.Published, "status");
    }

    public static void SeasonalRateSnapshotRejectsMovesFromRetired()
    {
        var item = SeasonalRateSnapshot.NewSeasonalRateSnapshot("code-932", "label-16");
        item.Status = SeasonalRateSnapshotStatus.Retired;
        Check.Fails(() => item.MoveTo(SeasonalRateSnapshotStatus.Created), "move");
    }

    public static void RunAll()
    {
        SeasonalRateSnapshotStartsInCreated();
        SeasonalRateSnapshotMovesToPublished();
        SeasonalRateSnapshotRejectsMovesFromRetired();
    }
}
