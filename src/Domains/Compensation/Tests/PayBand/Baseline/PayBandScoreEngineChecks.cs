namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandScoreEngineChecks
{
    public static void PayBandScoreEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandScoreEngine.DefaultPayBandScoreEngine();
        var item = PayBandScore.NewPayBandScore("code-828", "label-850");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandScoreEngineKeepsResultsWithinBounds();
    }
}
