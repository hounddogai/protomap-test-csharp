namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorAllocationEngineChecks
{
    public static void VisitorAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorAllocationEngine.DefaultVisitorAllocationEngine(Flags.NewRegistry());
        var item = VisitorAllocation.NewVisitorAllocation("code-514", "label-597");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorAllocationEngineKeepsResultsWithinBounds();
    }
}
