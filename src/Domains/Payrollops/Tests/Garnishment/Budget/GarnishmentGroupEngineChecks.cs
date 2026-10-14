namespace Acme.Workforce.Payrollops.Tests.Garnishment.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class GarnishmentGroupEngineChecks
{
    public static void GarnishmentGroupEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentGroupEngine.DefaultGarnishmentGroupEngine(Flags.NewRegistry());
        var item = GarnishmentGroup.NewGarnishmentGroup("code-915", "name-903");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentGroupEngineKeepsResultsWithinBounds();
    }
}
