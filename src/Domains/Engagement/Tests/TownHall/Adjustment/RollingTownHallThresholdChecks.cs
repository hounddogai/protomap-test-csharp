namespace Acme.Workforce.Engagement.Tests.TownHall.Adjustment;

using Acme.Workforce.Engagement.TownHall.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingTownHallThresholdChecks
{
    public static void RollingTownHallThresholdStartsInRequested()
    {
        var item = RollingTownHallThreshold.NewRollingTownHallThreshold("code-441", "title-155");
        Check.Equal(item.Status, RollingTownHallThresholdStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingTownHallThresholdMovesToRevoked()
    {
        var item = RollingTownHallThreshold.NewRollingTownHallThreshold("code-441", "title-155");
        try
        {
            item.MoveTo(RollingTownHallThresholdStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingTownHallThresholdStatus.Revoked, "status");
    }

    public static void RollingTownHallThresholdRejectsMovesFromExpired()
    {
        var item = RollingTownHallThreshold.NewRollingTownHallThreshold("code-441", "title-155");
        item.Status = RollingTownHallThresholdStatus.Expired;
        Check.Fails(() => item.MoveTo(RollingTownHallThresholdStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RollingTownHallThresholdStartsInRequested();
        RollingTownHallThresholdMovesToRevoked();
        RollingTownHallThresholdRejectsMovesFromExpired();
    }
}
