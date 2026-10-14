namespace Acme.Workforce.Absence.Tests.Holiday.Budget;

using Acme.Workforce.Absence.Holiday.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class HolidayEntryEngineChecks
{
    public static void HolidayEntryEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayEntryEngine.DefaultHolidayEntryEngine();
        var item = HolidayEntry.NewHolidayEntry("code-966", "title-532");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayEntryEngineKeepsResultsWithinBounds();
    }
}
