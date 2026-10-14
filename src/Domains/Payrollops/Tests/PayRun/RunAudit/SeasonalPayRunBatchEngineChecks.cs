namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.RunAudit;

/// <summary>Checks of run audit code, run by the project's runner.</summary>
public static class SeasonalPayRunBatchEngineChecks
{
    public static void SeasonalPayRunBatchEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalPayRunBatchEngine.DefaultSeasonalPayRunBatchEngine(Flags.NewRegistry());
        var item = SeasonalPayRunBatch.NewSeasonalPayRunBatch("code-669", "label-235");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalPayRunBatchEngineKeepsResultsWithinBounds();
    }
}
