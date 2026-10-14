namespace Acme.Workforce.Orgdesign.Tests.Span.Estimate;

using Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class GlobalSpanSlotEngineChecks
{
    public static void GlobalSpanSlotEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalSpanSlotEngine.DefaultGlobalSpanSlotEngine();
        var item = GlobalSpanSlot.NewGlobalSpanSlot("code-324", "title-867");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalSpanSlotEngineKeepsResultsWithinBounds();
    }
}
