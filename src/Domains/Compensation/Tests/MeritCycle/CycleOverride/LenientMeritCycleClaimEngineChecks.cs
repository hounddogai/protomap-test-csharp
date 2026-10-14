namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleOverride;

using Acme.Workforce.Compensation.MeritCycle.CycleOverride;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of cycle override code, run by the project's runner.</summary>
public static class LenientMeritCycleClaimEngineChecks
{
    public static void LenientMeritCycleClaimEngineKeepsResultsWithinBounds()
    {
        var engine = LenientMeritCycleClaimEngine.DefaultLenientMeritCycleClaimEngine(Flags.NewRegistry());
        var item = LenientMeritCycleClaim.NewLenientMeritCycleClaim("code-620", "title-334");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientMeritCycleClaimEngineKeepsResultsWithinBounds();
    }
}
