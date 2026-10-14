namespace Acme.Workforce.Engagement.Tests.TownHall.Exception;

using Acme.Workforce.Engagement.TownHall.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TownHallProfileChecks
{
    public static void TownHallProfileStartsInRequested()
    {
        var item = TownHallProfile.NewTownHallProfile("code-314", "label-507");
        Check.Equal(item.Status, TownHallProfileStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallProfileMovesToRevoked()
    {
        var item = TownHallProfile.NewTownHallProfile("code-314", "label-507");
        try
        {
            item.MoveTo(TownHallProfileStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallProfileStatus.Revoked, "status");
    }

    public static void TownHallProfileRejectsMovesFromExpired()
    {
        var item = TownHallProfile.NewTownHallProfile("code-314", "label-507");
        item.Status = TownHallProfileStatus.Expired;
        Check.Fails(() => item.MoveTo(TownHallProfileStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TownHallProfileStartsInRequested();
        TownHallProfileMovesToRevoked();
        TownHallProfileRejectsMovesFromExpired();
    }
}
