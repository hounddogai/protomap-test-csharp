namespace Acme.Workforce.Talent.Tests.Pool.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class DefaultPoolForecastEngineChecks
{
    public static void DefaultPoolForecastEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultPoolForecastEngine.DefaultDefaultPoolForecastEngine(Flags.NewRegistry());
        var item = DefaultPoolForecast.NewDefaultPoolForecast("code-50", "label-207");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultPoolForecastEngineKeepsResultsWithinBounds();
    }
}
