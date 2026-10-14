namespace Acme.Workforce.Compensation.Tests.PayBand.Summary;

using Acme.Workforce.Compensation.PayBand.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PayBandSummaryEngineChecks
{
    public static void PayBandSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandSummaryEngine.DefaultPayBandSummaryEngine(Flags.NewRegistry());
        var item = PayBandSummary.NewPayBandSummary("code-18", "title-11");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandSummaryEngineKeepsResultsWithinBounds();
    }
}
