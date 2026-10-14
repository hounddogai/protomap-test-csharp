namespace Acme.Workforce.Workplace.Tests.Desk.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class LenientDeskAllocationEngineChecks
{
    public static void LenientDeskAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = LenientDeskAllocationEngine.DefaultLenientDeskAllocationEngine(Flags.NewRegistry());
        var item = LenientDeskAllocation.NewLenientDeskAllocation("code-237", "title-827");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientDeskAllocationEngineKeepsResultsWithinBounds();
    }
}
