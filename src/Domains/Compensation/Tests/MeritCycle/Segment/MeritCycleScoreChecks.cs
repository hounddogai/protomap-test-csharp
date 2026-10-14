namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class MeritCycleScoreChecks
{
    public static void MeritCycleScoreStartsInRequested()
    {
        var item = MeritCycleScore.NewMeritCycleScore("code-739", "name-622");
        Check.Equal(item.Status, MeritCycleScoreStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleScoreMovesToGranted()
    {
        var item = MeritCycleScore.NewMeritCycleScore("code-739", "name-622");
        try
        {
            item.MoveTo(MeritCycleScoreStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleScoreStatus.Granted, "status");
    }

    public static void MeritCycleScoreRejectsMovesFromExpired()
    {
        var item = MeritCycleScore.NewMeritCycleScore("code-739", "name-622");
        item.Status = MeritCycleScoreStatus.Expired;
        Check.Fails(() => item.MoveTo(MeritCycleScoreStatus.Requested), "move");
    }

    public static void RunAll()
    {
        MeritCycleScoreStartsInRequested();
        MeritCycleScoreMovesToGranted();
        MeritCycleScoreRejectsMovesFromExpired();
    }
}
