namespace Acme.Workforce.Compensation.Tests.MeritCycle.Metric;

using Acme.Workforce.Compensation.MeritCycle.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedMeritCycleDigestChecks
{
    public static void CappedMeritCycleDigestStartsInRequested()
    {
        var item = CappedMeritCycleDigest.NewCappedMeritCycleDigest("code-293", "title-702");
        Check.Equal(item.Status, CappedMeritCycleDigestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedMeritCycleDigestMovesToGranted()
    {
        var item = CappedMeritCycleDigest.NewCappedMeritCycleDigest("code-293", "title-702");
        try
        {
            item.MoveTo(CappedMeritCycleDigestStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedMeritCycleDigestStatus.Granted, "status");
    }

    public static void CappedMeritCycleDigestRejectsMovesFromExpired()
    {
        var item = CappedMeritCycleDigest.NewCappedMeritCycleDigest("code-293", "title-702");
        item.Status = CappedMeritCycleDigestStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedMeritCycleDigestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedMeritCycleDigestStartsInRequested();
        CappedMeritCycleDigestMovesToGranted();
        CappedMeritCycleDigestRejectsMovesFromExpired();
    }
}
