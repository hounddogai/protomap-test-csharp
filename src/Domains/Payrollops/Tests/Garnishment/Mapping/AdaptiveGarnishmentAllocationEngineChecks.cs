namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentAllocationEngineChecks
{
    public static void AdaptiveGarnishmentAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveGarnishmentAllocationEngine.DefaultAdaptiveGarnishmentAllocationEngine();
        var item = AdaptiveGarnishmentAllocation.NewAdaptiveGarnishmentAllocation("code-445", "title-727");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentAllocationEngineKeepsResultsWithinBounds();
    }
}
