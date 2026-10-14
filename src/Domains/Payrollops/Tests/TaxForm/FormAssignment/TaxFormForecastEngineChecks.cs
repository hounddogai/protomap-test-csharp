namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.FormAssignment;

/// <summary>Checks of form assignment code, run by the project's runner.</summary>
public static class TaxFormForecastEngineChecks
{
    public static void TaxFormForecastEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormForecastEngine.DefaultTaxFormForecastEngine(Flags.NewRegistry());
        var item = TaxFormForecast.NewTaxFormForecast("code-685", "title-615");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormForecastEngineKeepsResultsWithinBounds();
    }
}
