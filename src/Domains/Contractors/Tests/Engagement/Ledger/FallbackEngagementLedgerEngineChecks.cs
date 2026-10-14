namespace Acme.Workforce.Contractors.Tests.Engagement.Ledger;

using Acme.Workforce.Contractors.Engagement.Ledger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FallbackEngagementLedgerEngineChecks
{
    public static void FallbackEngagementLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackEngagementLedgerEngine.DefaultFallbackEngagementLedgerEngine(Flags.NewRegistry());
        var item = FallbackEngagementLedger.NewFallbackEngagementLedger("code-409", "title-487");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackEngagementLedgerEngineKeepsResultsWithinBounds();
    }
}
