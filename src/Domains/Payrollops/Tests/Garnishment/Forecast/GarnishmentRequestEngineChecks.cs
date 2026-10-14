namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GarnishmentRequestEngineChecks
{
    public static void GarnishmentRequestEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentRequestEngine.DefaultGarnishmentRequestEngine(Flags.NewRegistry());
        var item = GarnishmentRequest.NewGarnishmentRequest("code-309", "title-295");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentRequestEngineKeepsResultsWithinBounds();
    }
}
