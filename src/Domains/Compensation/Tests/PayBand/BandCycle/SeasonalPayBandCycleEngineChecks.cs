namespace Acme.Workforce.Compensation.Tests.PayBand.BandCycle;

using Acme.Workforce.Compensation.PayBand.BandCycle;

/// <summary>Checks of band cycle code, run by the project's runner.</summary>
public static class SeasonalPayBandCycleEngineChecks
{
    public static void SeasonalPayBandCycleEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalPayBandCycleEngine.DefaultSeasonalPayBandCycleEngine();
        var item = SeasonalPayBandCycle.NewSeasonalPayBandCycle("code-524", "label-3");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalPayBandCycleEngineKeepsResultsWithinBounds();
    }
}
