namespace Acme.Workforce.Compensation.Tests.PayBand.Quota;

using Acme.Workforce.Compensation.PayBand.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class SeasonalPayBandCheckpointEngineChecks
{
    public static void SeasonalPayBandCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalPayBandCheckpointEngine.DefaultSeasonalPayBandCheckpointEngine();
        var item = SeasonalPayBandCheckpoint.NewSeasonalPayBandCheckpoint("code-962", "label-379");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalPayBandCheckpointEngineKeepsResultsWithinBounds();
    }
}
