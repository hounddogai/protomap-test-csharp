namespace Acme.Workforce.Workplace.Tests.Desk.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DeskTierEngineChecks
{
    public static void DeskTierEngineKeepsResultsWithinBounds()
    {
        var engine = DeskTierEngine.DefaultDeskTierEngine(Flags.NewRegistry());
        var item = DeskTier.NewDeskTier("code-790", "name-884");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskTierEngineKeepsResultsWithinBounds();
    }
}
