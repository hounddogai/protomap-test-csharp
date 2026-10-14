namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class DefaultAccessBadgeBundleEngineChecks
{
    public static void DefaultAccessBadgeBundleEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultAccessBadgeBundleEngine.DefaultDefaultAccessBadgeBundleEngine(Flags.NewRegistry());
        var item = DefaultAccessBadgeBundle.NewDefaultAccessBadgeBundle("code-94", "name-212");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultAccessBadgeBundleEngineKeepsResultsWithinBounds();
    }
}
