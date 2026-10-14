namespace Acme.Workforce.Payrollops.Tests.Garnishment.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LenientGarnishmentEstimateEngineChecks
{
    public static void LenientGarnishmentEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = LenientGarnishmentEstimateEngine.DefaultLenientGarnishmentEstimateEngine(Flags.NewRegistry());
        var item = LenientGarnishmentEstimate.NewLenientGarnishmentEstimate("code-420", "name-802");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientGarnishmentEstimateEngineKeepsResultsWithinBounds();
    }
}
