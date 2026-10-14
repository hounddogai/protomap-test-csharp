namespace Acme.Workforce.Orgdesign.Tests.Reorg.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ReorgAllocationEngineChecks
{
    public static void ReorgAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgAllocationEngine.DefaultReorgAllocationEngine(Flags.NewRegistry());
        var item = ReorgAllocation.NewReorgAllocation("code-961", "name-701");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgAllocationEngineKeepsResultsWithinBounds();
    }
}
