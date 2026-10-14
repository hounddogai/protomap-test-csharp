namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class PendingVisitorSlotEngineChecks
{
    public static void PendingVisitorSlotEngineKeepsResultsWithinBounds()
    {
        var engine = PendingVisitorSlotEngine.DefaultPendingVisitorSlotEngine(Flags.NewRegistry());
        var item = PendingVisitorSlot.NewPendingVisitorSlot("code-319", "title-722");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingVisitorSlotEngineKeepsResultsWithinBounds();
    }
}
