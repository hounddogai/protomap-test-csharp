namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdaptiveTownHallManifestChecks
{
    public static void AdaptiveTownHallManifestStartsInProposed()
    {
        var item = AdaptiveTownHallManifest.NewAdaptiveTownHallManifest("code-25", "name-668");
        Check.Equal(item.Status, AdaptiveTownHallManifestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveTownHallManifestMovesToApproved()
    {
        var item = AdaptiveTownHallManifest.NewAdaptiveTownHallManifest("code-25", "name-668");
        try
        {
            item.MoveTo(AdaptiveTownHallManifestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveTownHallManifestStatus.Approved, "status");
    }

    public static void AdaptiveTownHallManifestRejectsMovesFromWithdrawn()
    {
        var item = AdaptiveTownHallManifest.NewAdaptiveTownHallManifest("code-25", "name-668");
        item.Status = AdaptiveTownHallManifestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AdaptiveTownHallManifestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AdaptiveTownHallManifestStartsInProposed();
        AdaptiveTownHallManifestMovesToApproved();
        AdaptiveTownHallManifestRejectsMovesFromWithdrawn();
    }
}
