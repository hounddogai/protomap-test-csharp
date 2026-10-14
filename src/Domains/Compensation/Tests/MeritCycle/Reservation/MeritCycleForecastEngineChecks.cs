namespace Acme.Workforce.Compensation.Tests.MeritCycle.Reservation;

using Acme.Workforce.Compensation.MeritCycle.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MeritCycleForecastEngineChecks
{
    public static void MeritCycleForecastEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleForecastEngine.DefaultMeritCycleForecastEngine(Flags.NewRegistry());
        var item = MeritCycleForecast.NewMeritCycleForecast("code-807", "title-23");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleForecastEngineKeepsResultsWithinBounds();
    }
}
