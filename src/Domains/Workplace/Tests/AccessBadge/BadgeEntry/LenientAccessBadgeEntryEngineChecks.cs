namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Checks of badge entry code, run by the project's runner.</summary>
public static class LenientAccessBadgeEntryEngineChecks
{
    public static void LenientAccessBadgeEntryEngineKeepsResultsWithinBounds()
    {
        var engine = LenientAccessBadgeEntryEngine.DefaultLenientAccessBadgeEntryEngine();
        var item = LenientAccessBadgeEntry.NewLenientAccessBadgeEntry("code-94", "title-131");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientAccessBadgeEntryEngineKeepsResultsWithinBounds();
    }
}
