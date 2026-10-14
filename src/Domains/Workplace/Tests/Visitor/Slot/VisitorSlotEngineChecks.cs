namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorSlotEngineChecks
{
    public static void VisitorSlotEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorSlotEngine.DefaultVisitorSlotEngine(Flags.NewRegistry());
        var item = VisitorSlot.NewVisitorSlot("code-465", "label-865");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorSlotEngineKeepsResultsWithinBounds();
    }
}
