namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAllocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Checks of run allocation code, run by the project's runner.</summary>
public static class FallbackPayRunCycleEngineChecks
{
    public static void FallbackPayRunCycleEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackPayRunCycleEngine.DefaultFallbackPayRunCycleEngine(Flags.NewRegistry());
        var item = FallbackPayRunCycle.NewFallbackPayRunCycle("code-163", "title-577");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackPayRunCycleEngineKeepsResultsWithinBounds();
    }
}
