namespace Acme.Workforce.Contractors.Tests.Engagement.Ledger;

using Acme.Workforce.Contractors.Engagement.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CappedEngagementEntryEngineChecks
{
    public static void CappedEngagementEntryEngineKeepsResultsWithinBounds()
    {
        var engine = CappedEngagementEntryEngine.DefaultCappedEngagementEntryEngine();
        var item = CappedEngagementEntry.NewCappedEngagementEntry("code-528", "label-180");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedEngagementEntryEngineKeepsResultsWithinBounds();
    }
}
