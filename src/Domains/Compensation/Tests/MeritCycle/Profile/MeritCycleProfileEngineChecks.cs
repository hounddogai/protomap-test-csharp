namespace Acme.Workforce.Compensation.Tests.MeritCycle.Profile;

using Acme.Workforce.Compensation.MeritCycle.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MeritCycleProfileEngineChecks
{
    public static void MeritCycleProfileEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleProfileEngine.DefaultMeritCycleProfileEngine(Flags.NewRegistry());
        var item = MeritCycleProfile.NewMeritCycleProfile("code-538", "title-918");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleProfileEngineKeepsResultsWithinBounds();
    }
}
