namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class AccessBadgeVarianceEngineChecks
{
    public static void AccessBadgeVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeVarianceEngine.DefaultAccessBadgeVarianceEngine(Flags.NewRegistry());
        var item = AccessBadgeVariance.NewAccessBadgeVariance("code-595", "name-203");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeVarianceEngineKeepsResultsWithinBounds();
    }
}
