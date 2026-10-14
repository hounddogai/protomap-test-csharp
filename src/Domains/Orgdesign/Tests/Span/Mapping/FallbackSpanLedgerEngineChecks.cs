namespace Acme.Workforce.Orgdesign.Tests.Span.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class FallbackSpanLedgerEngineChecks
{
    public static void FallbackSpanLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackSpanLedgerEngine.DefaultFallbackSpanLedgerEngine(Flags.NewRegistry());
        var item = FallbackSpanLedger.NewFallbackSpanLedger("code-268", "name-932");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackSpanLedgerEngineKeepsResultsWithinBounds();
    }
}
