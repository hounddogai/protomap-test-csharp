namespace Acme.Workforce.Orgdesign.Tests.Span.Entry;

using Acme.Workforce.Orgdesign.Span.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class GlobalSpanEntryEngineChecks
{
    public static void GlobalSpanEntryEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalSpanEntryEngine.DefaultGlobalSpanEntryEngine();
        var item = GlobalSpanEntry.NewGlobalSpanEntry("code-524", "name-922");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalSpanEntryEngineKeepsResultsWithinBounds();
    }
}
