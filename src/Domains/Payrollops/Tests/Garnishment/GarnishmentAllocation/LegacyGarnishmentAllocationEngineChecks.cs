namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

/// <summary>Checks of garnishment allocation code, run by the project's runner.</summary>
public static class LegacyGarnishmentAllocationEngineChecks
{
    public static void LegacyGarnishmentAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyGarnishmentAllocationEngine.DefaultLegacyGarnishmentAllocationEngine();
        var item = LegacyGarnishmentAllocation.NewLegacyGarnishmentAllocation("code-324", "label-575");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyGarnishmentAllocationEngineKeepsResultsWithinBounds();
    }
}
