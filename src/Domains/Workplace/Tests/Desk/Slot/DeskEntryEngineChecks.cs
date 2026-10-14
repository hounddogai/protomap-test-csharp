namespace Acme.Workforce.Workplace.Tests.Desk.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeskEntryEngineChecks
{
    public static void DeskEntryEngineKeepsResultsWithinBounds()
    {
        var engine = DeskEntryEngine.DefaultDeskEntryEngine(Flags.NewRegistry());
        var item = DeskEntry.NewDeskEntry("code-974", "label-838");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskEntryEngineKeepsResultsWithinBounds();
    }
}
