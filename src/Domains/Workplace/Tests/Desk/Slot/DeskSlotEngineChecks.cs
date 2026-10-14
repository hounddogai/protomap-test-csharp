namespace Acme.Workforce.Workplace.Tests.Desk.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeskSlotEngineChecks
{
    public static void DeskSlotEngineKeepsResultsWithinBounds()
    {
        var engine = DeskSlotEngine.DefaultDeskSlotEngine(Flags.NewRegistry());
        var item = DeskSlot.NewDeskSlot("code-151", "label-66");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskSlotEngineKeepsResultsWithinBounds();
    }
}
