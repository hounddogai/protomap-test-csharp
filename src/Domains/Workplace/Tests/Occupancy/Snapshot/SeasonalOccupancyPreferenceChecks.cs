namespace Acme.Workforce.Workplace.Tests.Occupancy.Snapshot;

using Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalOccupancyPreferenceChecks
{
    public static void SeasonalOccupancyPreferenceStartsInPlanned()
    {
        var item = SeasonalOccupancyPreference.NewSeasonalOccupancyPreference("code-131", "label-301");
        Check.Equal(item.Status, SeasonalOccupancyPreferenceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalOccupancyPreferenceMovesToScheduled()
    {
        var item = SeasonalOccupancyPreference.NewSeasonalOccupancyPreference("code-131", "label-301");
        try
        {
            item.MoveTo(SeasonalOccupancyPreferenceStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalOccupancyPreferenceStatus.Scheduled, "status");
    }

    public static void SeasonalOccupancyPreferenceRejectsMovesFromCancelled()
    {
        var item = SeasonalOccupancyPreference.NewSeasonalOccupancyPreference("code-131", "label-301");
        item.Status = SeasonalOccupancyPreferenceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalOccupancyPreferenceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalOccupancyPreferenceStartsInPlanned();
        SeasonalOccupancyPreferenceMovesToScheduled();
        SeasonalOccupancyPreferenceRejectsMovesFromCancelled();
    }
}
