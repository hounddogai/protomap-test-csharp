namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class HolidaySlotEngineChecks
{
    public static void HolidaySlotEngineKeepsResultsWithinBounds()
    {
        var engine = HolidaySlotEngine.DefaultHolidaySlotEngine();
        var item = HolidaySlot.NewHolidaySlot("code-354", "title-424");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidaySlotEngineKeepsResultsWithinBounds();
    }
}
