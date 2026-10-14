namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleEntryEngineChecks
{
    public static void MeritCycleEntryEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleEntryEngine.DefaultMeritCycleEntryEngine(Flags.NewRegistry());
        var item = MeritCycleEntry.NewMeritCycleEntry("code-190", "title-788");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleEntryEngineKeepsResultsWithinBounds();
    }
}
