namespace Acme.Workforce.Workplace.Tests.Visitor.Entry;

using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class VisitorEntryEngineChecks
{
    public static void VisitorEntryEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorEntryEngine.DefaultVisitorEntryEngine();
        var item = VisitorEntry.NewVisitorEntry("code-70", "title-947");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorEntryEngineKeepsResultsWithinBounds();
    }
}
