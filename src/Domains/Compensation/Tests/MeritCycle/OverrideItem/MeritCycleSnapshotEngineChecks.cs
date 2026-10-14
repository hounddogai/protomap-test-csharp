namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleSnapshotEngineChecks
{
    public static void MeritCycleSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleSnapshotEngine.DefaultMeritCycleSnapshotEngine();
        var item = MeritCycleSnapshot.NewMeritCycleSnapshot("code-799", "label-457");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleSnapshotEngineKeepsResultsWithinBounds();
    }
}
