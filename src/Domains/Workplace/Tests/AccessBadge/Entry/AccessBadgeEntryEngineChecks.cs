namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeEntryEngineChecks
{
    public static void AccessBadgeEntryEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeEntryEngine.DefaultAccessBadgeEntryEngine(Flags.NewRegistry());
        var item = AccessBadgeEntry.NewAccessBadgeEntry("code-532", "label-819");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeEntryEngineKeepsResultsWithinBounds();
    }
}
