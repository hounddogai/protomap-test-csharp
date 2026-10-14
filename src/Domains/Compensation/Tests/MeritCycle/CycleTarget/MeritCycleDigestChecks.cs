namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleTarget;

using Acme.Workforce.Compensation.MeritCycle.CycleTarget;

/// <summary>Checks of cycle target code, run by the project's runner.</summary>
public static class MeritCycleDigestChecks
{
    public static void MeritCycleDigestStartsInIdle()
    {
        var item = MeritCycleDigest.NewMeritCycleDigest("code-321", "name-858");
        Check.Equal(item.Status, MeritCycleDigestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleDigestMovesToStopped()
    {
        var item = MeritCycleDigest.NewMeritCycleDigest("code-321", "name-858");
        try
        {
            item.MoveTo(MeritCycleDigestStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleDigestStatus.Stopped, "status");
    }

    public static void MeritCycleDigestRejectsMovesFromStopped()
    {
        var item = MeritCycleDigest.NewMeritCycleDigest("code-321", "name-858");
        item.Status = MeritCycleDigestStatus.Stopped;
        Check.Fails(() => item.MoveTo(MeritCycleDigestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        MeritCycleDigestStartsInIdle();
        MeritCycleDigestMovesToStopped();
        MeritCycleDigestRejectsMovesFromStopped();
    }
}
