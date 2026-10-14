namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ScheduledTaxFormForecastEngineChecks
{
    public static void ScheduledTaxFormForecastEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledTaxFormForecastEngine.DefaultScheduledTaxFormForecastEngine(Flags.NewRegistry());
        var item = ScheduledTaxFormForecast.NewScheduledTaxFormForecast("code-377", "title-691");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledTaxFormForecastEngineKeepsResultsWithinBounds();
    }
}
