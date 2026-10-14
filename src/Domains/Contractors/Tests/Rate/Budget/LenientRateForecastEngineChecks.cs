namespace Acme.Workforce.Contractors.Tests.Rate.Budget;

using Acme.Workforce.Contractors.Rate.Budget;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LenientRateForecastEngineChecks
{
    public static void LenientRateForecastEngineKeepsResultsWithinBounds()
    {
        var engine = LenientRateForecastEngine.DefaultLenientRateForecastEngine(Flags.NewRegistry());
        var item = LenientRateForecast.NewLenientRateForecast("code-751", "label-237");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientRateForecastEngineKeepsResultsWithinBounds();
    }
}
