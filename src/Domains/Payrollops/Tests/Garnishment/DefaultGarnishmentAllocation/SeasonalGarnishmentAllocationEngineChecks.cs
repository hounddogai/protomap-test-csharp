namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class SeasonalGarnishmentAllocationEngineChecks
{
    public static void SeasonalGarnishmentAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalGarnishmentAllocationEngine.DefaultSeasonalGarnishmentAllocationEngine(Flags.NewRegistry());
        var item = SeasonalGarnishmentAllocation.NewSeasonalGarnishmentAllocation("code-406", "title-465");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalGarnishmentAllocationEngineKeepsResultsWithinBounds();
    }
}
