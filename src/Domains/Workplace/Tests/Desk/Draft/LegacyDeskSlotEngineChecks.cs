namespace Acme.Workforce.Workplace.Tests.Desk.Draft;

using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LegacyDeskSlotEngineChecks
{
    public static void LegacyDeskSlotEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyDeskSlotEngine.DefaultLegacyDeskSlotEngine();
        var item = LegacyDeskSlot.NewLegacyDeskSlot("code-321", "title-487");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyDeskSlotEngineKeepsResultsWithinBounds();
    }
}
