namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RollingMeritCycleOverrideEngineChecks
{
    public static void RollingMeritCycleOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = RollingMeritCycleOverrideEngine.DefaultRollingMeritCycleOverrideEngine(Flags.NewRegistry());
        var item = RollingMeritCycleOverride.NewRollingMeritCycleOverride("code-282", "name-900");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingMeritCycleOverrideEngineKeepsResultsWithinBounds();
    }
}
