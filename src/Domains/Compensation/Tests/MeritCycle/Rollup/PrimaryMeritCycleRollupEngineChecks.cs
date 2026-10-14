namespace Acme.Workforce.Compensation.Tests.MeritCycle.Rollup;

using Acme.Workforce.Compensation.MeritCycle.Rollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryMeritCycleRollupEngineChecks
{
    public static void PrimaryMeritCycleRollupEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryMeritCycleRollupEngine.DefaultPrimaryMeritCycleRollupEngine(Flags.NewRegistry());
        var item = PrimaryMeritCycleRollup.NewPrimaryMeritCycleRollup("code-787", "title-879");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryMeritCycleRollupEngineKeepsResultsWithinBounds();
    }
}
