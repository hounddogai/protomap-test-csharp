namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PendingDeskEntryEngineChecks
{
    public static void PendingDeskEntryEngineKeepsResultsWithinBounds()
    {
        var engine = PendingDeskEntryEngine.DefaultPendingDeskEntryEngine();
        var item = PendingDeskEntry.NewPendingDeskEntry("code-230", "label-39");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingDeskEntryEngineKeepsResultsWithinBounds();
    }
}
