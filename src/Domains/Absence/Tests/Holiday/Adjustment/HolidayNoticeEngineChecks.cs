namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HolidayNoticeEngineChecks
{
    public static void HolidayNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayNoticeEngine.DefaultHolidayNoticeEngine();
        var item = HolidayNotice.NewHolidayNotice("code-663", "name-387");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayNoticeEngineKeepsResultsWithinBounds();
    }
}
