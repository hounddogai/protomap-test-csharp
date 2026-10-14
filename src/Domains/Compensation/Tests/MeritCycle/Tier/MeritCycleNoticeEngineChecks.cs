namespace Acme.Workforce.Compensation.Tests.MeritCycle.Tier;

using Acme.Workforce.Compensation.MeritCycle.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MeritCycleNoticeEngineChecks
{
    public static void MeritCycleNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleNoticeEngine.DefaultMeritCycleNoticeEngine();
        var item = MeritCycleNotice.NewMeritCycleNotice("code-186", "label-683");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleNoticeEngineKeepsResultsWithinBounds();
    }
}
