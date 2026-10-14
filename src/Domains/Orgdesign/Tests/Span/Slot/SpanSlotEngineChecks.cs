namespace Acme.Workforce.Orgdesign.Tests.Span.Slot;

using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SpanSlotEngineChecks
{
    public static void SpanSlotEngineKeepsResultsWithinBounds()
    {
        var engine = SpanSlotEngine.DefaultSpanSlotEngine();
        var item = SpanSlot.NewSpanSlot("code-431", "label-149");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanSlotEngineKeepsResultsWithinBounds();
    }
}
