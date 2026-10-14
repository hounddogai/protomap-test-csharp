namespace Acme.Workforce.Payrollops.Tests.Garnishment.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class GarnishmentEstimateEngineChecks
{
    public static void GarnishmentEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentEstimateEngine.DefaultGarnishmentEstimateEngine(Flags.NewRegistry());
        var item = GarnishmentEstimate.NewGarnishmentEstimate("code-805", "name-566");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentEstimateEngineKeepsResultsWithinBounds();
    }
}
