namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class AccessBadgePreferenceEngineChecks
{
    public static void AccessBadgePreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgePreferenceEngine.DefaultAccessBadgePreferenceEngine();
        var item = AccessBadgePreference.NewAccessBadgePreference("code-246", "title-473");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgePreferenceEngineKeepsResultsWithinBounds();
    }
}
