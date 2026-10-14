namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class WeightedTownHallManifestChecks
{
    public static void WeightedTownHallManifestStartsInCreated()
    {
        var item = WeightedTownHallManifest.NewWeightedTownHallManifest("code-746", "name-118");
        Check.Equal(item.Status, WeightedTownHallManifestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedTownHallManifestMovesToPublished()
    {
        var item = WeightedTownHallManifest.NewWeightedTownHallManifest("code-746", "name-118");
        try
        {
            item.MoveTo(WeightedTownHallManifestStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedTownHallManifestStatus.Published, "status");
    }

    public static void WeightedTownHallManifestRejectsMovesFromRetired()
    {
        var item = WeightedTownHallManifest.NewWeightedTownHallManifest("code-746", "name-118");
        item.Status = WeightedTownHallManifestStatus.Retired;
        Check.Fails(() => item.MoveTo(WeightedTownHallManifestStatus.Created), "move");
    }

    public static void RunAll()
    {
        WeightedTownHallManifestStartsInCreated();
        WeightedTownHallManifestMovesToPublished();
        WeightedTownHallManifestRejectsMovesFromRetired();
    }
}
