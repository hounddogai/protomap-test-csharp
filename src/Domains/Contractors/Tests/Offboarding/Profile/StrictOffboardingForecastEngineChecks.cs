namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class StrictOffboardingForecastEngineChecks
{
    public static void StrictOffboardingForecastEngineKeepsResultsWithinBounds()
    {
        var engine = StrictOffboardingForecastEngine.DefaultStrictOffboardingForecastEngine(Flags.NewRegistry());
        var item = StrictOffboardingForecast.NewStrictOffboardingForecast("code-838", "title-913");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictOffboardingForecastEngineKeepsResultsWithinBounds();
    }
}
