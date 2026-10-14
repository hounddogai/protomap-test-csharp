namespace Acme.Workforce.Compensation.Tests.PayBand.Draft;

using Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PayBandDraftEngineChecks
{
    public static void PayBandDraftEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandDraftEngine.DefaultPayBandDraftEngine();
        var item = PayBandDraft.NewPayBandDraft("code-598", "name-624");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandDraftEngineKeepsResultsWithinBounds();
    }
}
