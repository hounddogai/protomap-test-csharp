namespace Acme.Workforce.Engagement.Tests.TownHall.OverrideItem;

using Acme.Workforce.Engagement.TownHall.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TownHallOverrideChecks
{
    public static void TownHallOverrideStartsInRequested()
    {
        var item = TownHallOverride.NewTownHallOverride("code-520", "title-865");
        Check.Equal(item.Status, TownHallOverrideStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallOverrideMovesToRevoked()
    {
        var item = TownHallOverride.NewTownHallOverride("code-520", "title-865");
        try
        {
            item.MoveTo(TownHallOverrideStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallOverrideStatus.Revoked, "status");
    }

    public static void TownHallOverrideRejectsMovesFromExpired()
    {
        var item = TownHallOverride.NewTownHallOverride("code-520", "title-865");
        item.Status = TownHallOverrideStatus.Expired;
        Check.Fails(() => item.MoveTo(TownHallOverrideStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TownHallOverrideStartsInRequested();
        TownHallOverrideMovesToRevoked();
        TownHallOverrideRejectsMovesFromExpired();
    }
}
