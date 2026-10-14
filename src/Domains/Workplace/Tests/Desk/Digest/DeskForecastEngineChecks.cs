namespace Acme.Workforce.Workplace.Tests.Desk.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class DeskForecastEngineChecks
{
    public static void DeskForecastEngineKeepsResultsWithinBounds()
    {
        var engine = DeskForecastEngine.DefaultDeskForecastEngine(Flags.NewRegistry());
        var item = DeskForecast.NewDeskForecast("code-116", "title-560");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskForecastEngineKeepsResultsWithinBounds();
    }
}
