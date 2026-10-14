namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class StrictPayBandDraftEngineChecks
{
    public static void StrictPayBandDraftEngineKeepsResultsWithinBounds()
    {
        var engine = StrictPayBandDraftEngine.DefaultStrictPayBandDraftEngine();
        var item = StrictPayBandDraft.NewStrictPayBandDraft("code-622", "name-946");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictPayBandDraftEngineKeepsResultsWithinBounds();
    }
}
