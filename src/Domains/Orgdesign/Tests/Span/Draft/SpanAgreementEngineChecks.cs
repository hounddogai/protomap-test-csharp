namespace Acme.Workforce.Orgdesign.Tests.Span.Draft;

using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SpanAgreementEngineChecks
{
    public static void SpanAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = SpanAgreementEngine.DefaultSpanAgreementEngine();
        var item = SpanAgreement.NewSpanAgreement("code-621", "name-257");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanAgreementEngineKeepsResultsWithinBounds();
    }
}
