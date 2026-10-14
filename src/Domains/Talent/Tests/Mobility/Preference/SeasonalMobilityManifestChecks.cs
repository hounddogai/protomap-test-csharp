namespace Acme.Workforce.Talent.Tests.Mobility.Preference;

using Acme.Workforce.Talent.Mobility.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SeasonalMobilityManifestChecks
{
    public static void SeasonalMobilityManifestStartsInRequested()
    {
        var item = SeasonalMobilityManifest.NewSeasonalMobilityManifest("code-112", "name-471");
        Check.Equal(item.Status, SeasonalMobilityManifestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalMobilityManifestMovesToGranted()
    {
        var item = SeasonalMobilityManifest.NewSeasonalMobilityManifest("code-112", "name-471");
        try
        {
            item.MoveTo(SeasonalMobilityManifestStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalMobilityManifestStatus.Granted, "status");
    }

    public static void SeasonalMobilityManifestRejectsMovesFromExpired()
    {
        var item = SeasonalMobilityManifest.NewSeasonalMobilityManifest("code-112", "name-471");
        item.Status = SeasonalMobilityManifestStatus.Expired;
        Check.Fails(() => item.MoveTo(SeasonalMobilityManifestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SeasonalMobilityManifestStartsInRequested();
        SeasonalMobilityManifestMovesToGranted();
        SeasonalMobilityManifestRejectsMovesFromExpired();
    }
}
