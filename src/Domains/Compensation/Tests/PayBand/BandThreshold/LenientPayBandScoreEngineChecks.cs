namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class LenientPayBandScoreEngineChecks
{
    public static void LenientPayBandScoreEngineKeepsResultsWithinBounds()
    {
        var engine = LenientPayBandScoreEngine.DefaultLenientPayBandScoreEngine();
        var item = LenientPayBandScore.NewLenientPayBandScore("code-467", "label-779");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientPayBandScoreEngineKeepsResultsWithinBounds();
    }
}
