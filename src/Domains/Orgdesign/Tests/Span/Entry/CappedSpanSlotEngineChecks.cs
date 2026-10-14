namespace Acme.Workforce.Orgdesign.Tests.Span.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CappedSpanSlotEngineChecks
{
    public static void CappedSpanSlotEngineKeepsResultsWithinBounds()
    {
        var engine = CappedSpanSlotEngine.DefaultCappedSpanSlotEngine(Flags.NewRegistry());
        var item = CappedSpanSlot.NewCappedSpanSlot("code-890", "label-457");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedSpanSlotEngineKeepsResultsWithinBounds();
    }
}
