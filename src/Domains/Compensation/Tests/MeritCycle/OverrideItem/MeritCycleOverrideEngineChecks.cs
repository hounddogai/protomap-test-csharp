namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleOverrideEngineChecks
{
    public static void MeritCycleOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleOverrideEngine.DefaultMeritCycleOverrideEngine(Flags.NewRegistry());
        var item = MeritCycleOverride.NewMeritCycleOverride("code-517", "label-833");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleOverrideEngineKeepsResultsWithinBounds();
    }
}
