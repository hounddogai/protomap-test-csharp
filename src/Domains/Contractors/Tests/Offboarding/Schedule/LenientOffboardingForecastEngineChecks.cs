namespace Acme.Workforce.Contractors.Tests.Offboarding.Schedule;

using Acme.Workforce.Contractors.Offboarding.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LenientOffboardingForecastEngineChecks
{
    public static void LenientOffboardingForecastEngineKeepsResultsWithinBounds()
    {
        var engine = LenientOffboardingForecastEngine.DefaultLenientOffboardingForecastEngine();
        var item = LenientOffboardingForecast.NewLenientOffboardingForecast("code-67", "name-379");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientOffboardingForecastEngineKeepsResultsWithinBounds();
    }
}
