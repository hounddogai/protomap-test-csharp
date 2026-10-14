namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TownHallDigestChecks
{
    public static void TownHallDigestStartsInProposed()
    {
        var item = TownHallDigest.NewTownHallDigest("code-57", "label-655");
        Check.Equal(item.Status, TownHallDigestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallDigestMovesToApproved()
    {
        var item = TownHallDigest.NewTownHallDigest("code-57", "label-655");
        try
        {
            item.MoveTo(TownHallDigestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallDigestStatus.Approved, "status");
    }

    public static void TownHallDigestRejectsMovesFromWithdrawn()
    {
        var item = TownHallDigest.NewTownHallDigest("code-57", "label-655");
        item.Status = TownHallDigestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TownHallDigestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TownHallDigestStartsInProposed();
        TownHallDigestMovesToApproved();
        TownHallDigestRejectsMovesFromWithdrawn();
    }
}
