namespace Acme.Workforce.Compensation.Tests.MeritCycle.Claim;

using Acme.Workforce.Compensation.MeritCycle.Claim;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class GlobalMeritCycleRollupEngineChecks
{
    public static void GlobalMeritCycleRollupEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalMeritCycleRollupEngine.DefaultGlobalMeritCycleRollupEngine(Flags.NewRegistry());
        var item = GlobalMeritCycleRollup.NewGlobalMeritCycleRollup("code-244", "name-185");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalMeritCycleRollupEngineKeepsResultsWithinBounds();
    }
}
