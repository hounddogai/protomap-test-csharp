namespace Acme.Workforce.Workplace.Tests.Occupancy.Preference;

using Acme.Workforce.Workplace.Occupancy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class OccupancyPreferenceChecks
{
    public static void OccupancyPreferenceStartsInRequested()
    {
        var item = OccupancyPreference.NewOccupancyPreference("code-487", "title-703");
        Check.Equal(item.Status, OccupancyPreferenceStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyPreferenceMovesToGranted()
    {
        var item = OccupancyPreference.NewOccupancyPreference("code-487", "title-703");
        try
        {
            item.MoveTo(OccupancyPreferenceStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyPreferenceStatus.Granted, "status");
    }

    public static void OccupancyPreferenceRejectsMovesFromExpired()
    {
        var item = OccupancyPreference.NewOccupancyPreference("code-487", "title-703");
        item.Status = OccupancyPreferenceStatus.Expired;
        Check.Fails(() => item.MoveTo(OccupancyPreferenceStatus.Requested), "move");
    }

    public static void RunAll()
    {
        OccupancyPreferenceStartsInRequested();
        OccupancyPreferenceMovesToGranted();
        OccupancyPreferenceRejectsMovesFromExpired();
    }
}
