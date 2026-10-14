namespace Acme.Workforce.Contractors.Tests.Rate.Tier;

using Acme.Workforce.Contractors.Rate.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RateLedgerEngineChecks
{
    public static void RateLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = RateLedgerEngine.DefaultRateLedgerEngine();
        var item = RateLedger.NewRateLedger("code-701", "name-229");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateLedgerEngineKeepsResultsWithinBounds();
    }
}
