namespace Acme.Workforce.Workplace.Tests.Desk.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PrimaryDeskForecastEngineChecks
{
    public static void PrimaryDeskForecastEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryDeskForecastEngine.DefaultPrimaryDeskForecastEngine(Flags.NewRegistry());
        var item = PrimaryDeskForecast.NewPrimaryDeskForecast("code-737", "label-320");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryDeskForecastEngineKeepsResultsWithinBounds();
    }
}
