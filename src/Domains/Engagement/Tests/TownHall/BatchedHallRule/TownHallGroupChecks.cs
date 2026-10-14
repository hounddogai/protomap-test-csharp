namespace Acme.Workforce.Engagement.Tests.TownHall.BatchedHallRule;

using Acme.Workforce.Engagement.TownHall.BatchedHallRule;

/// <summary>Checks of batched hall rule code, run by the project's runner.</summary>
public static class TownHallGroupChecks
{
    public static void TownHallGroupStartsInRequested()
    {
        var item = TownHallGroup.NewTownHallGroup("code-346", "label-873");
        Check.Equal(item.Status, TownHallGroupStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallGroupMovesToRevoked()
    {
        var item = TownHallGroup.NewTownHallGroup("code-346", "label-873");
        try
        {
            item.MoveTo(TownHallGroupStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallGroupStatus.Revoked, "status");
    }

    public static void TownHallGroupRejectsMovesFromExpired()
    {
        var item = TownHallGroup.NewTownHallGroup("code-346", "label-873");
        item.Status = TownHallGroupStatus.Expired;
        Check.Fails(() => item.MoveTo(TownHallGroupStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TownHallGroupStartsInRequested();
        TownHallGroupMovesToRevoked();
        TownHallGroupRejectsMovesFromExpired();
    }
}
