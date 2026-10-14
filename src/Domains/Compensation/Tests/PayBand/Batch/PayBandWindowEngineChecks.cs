namespace Acme.Workforce.Compensation.Tests.PayBand.Batch;

using Acme.Workforce.Compensation.PayBand.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PayBandWindowEngineChecks
{
    public static void PayBandWindowEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandWindowEngine.DefaultPayBandWindowEngine();
        var item = PayBandWindow.NewPayBandWindow("code-186", "title-78");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandWindowEngineKeepsResultsWithinBounds();
    }
}
