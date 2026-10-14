namespace Acme.Workforce.Orgdesign.Tests.Span.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LenientSpanLedgerEngineChecks
{
    public static void LenientSpanLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = LenientSpanLedgerEngine.DefaultLenientSpanLedgerEngine(Flags.NewRegistry());
        var item = LenientSpanLedger.NewLenientSpanLedger("code-158", "label-451");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientSpanLedgerEngineKeepsResultsWithinBounds();
    }
}
