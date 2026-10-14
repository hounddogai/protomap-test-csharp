namespace Acme.Workforce.Workplace.Tests.Desk.DeskAllocation;

using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Checks of desk allocation code, run by the project's runner.</summary>
public static class LegacyDeskEntryEngineChecks
{
    public static void LegacyDeskEntryEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyDeskEntryEngine.DefaultLegacyDeskEntryEngine();
        var item = LegacyDeskEntry.NewLegacyDeskEntry("code-228", "title-166");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyDeskEntryEngineKeepsResultsWithinBounds();
    }
}
