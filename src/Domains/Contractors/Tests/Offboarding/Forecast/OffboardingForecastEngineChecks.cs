namespace Acme.Workforce.Contractors.Tests.Offboarding.Forecast;

using Acme.Workforce.Contractors.Offboarding.Forecast;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class OffboardingForecastEngineChecks
{
    public static void OffboardingForecastEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingForecastEngine.DefaultOffboardingForecastEngine(Flags.NewRegistry());
        var item = OffboardingForecast.NewOffboardingForecast("code-119", "label-128");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingForecastEngineKeepsResultsWithinBounds();
    }
}
