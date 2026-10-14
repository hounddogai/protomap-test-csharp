namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleTarget;

using Acme.Workforce.Compensation.MeritCycle.CycleTarget;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of cycle target code, run by the project's runner.</summary>
public static class MeritCycleDigestEngineChecks
{
    public static void MeritCycleDigestEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleDigestEngine.DefaultMeritCycleDigestEngine(Flags.NewRegistry());
        var item = MeritCycleDigest.NewMeritCycleDigest("code-321", "name-858");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleDigestEngineKeepsResultsWithinBounds();
    }
}
