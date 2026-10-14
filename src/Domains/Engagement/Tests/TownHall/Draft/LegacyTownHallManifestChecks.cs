namespace Acme.Workforce.Engagement.Tests.TownHall.Draft;

using Acme.Workforce.Engagement.TownHall.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LegacyTownHallManifestChecks
{
    public static void LegacyTownHallManifestStartsInPlanned()
    {
        var item = LegacyTownHallManifest.NewLegacyTownHallManifest("code-312", "name-306");
        Check.Equal(item.Status, LegacyTownHallManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyTownHallManifestMovesToCompleted()
    {
        var item = LegacyTownHallManifest.NewLegacyTownHallManifest("code-312", "name-306");
        try
        {
            item.MoveTo(LegacyTownHallManifestStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyTownHallManifestStatus.Completed, "status");
    }

    public static void LegacyTownHallManifestRejectsMovesFromCancelled()
    {
        var item = LegacyTownHallManifest.NewLegacyTownHallManifest("code-312", "name-306");
        item.Status = LegacyTownHallManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LegacyTownHallManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LegacyTownHallManifestStartsInPlanned();
        LegacyTownHallManifestMovesToCompleted();
        LegacyTownHallManifestRejectsMovesFromCancelled();
    }
}
