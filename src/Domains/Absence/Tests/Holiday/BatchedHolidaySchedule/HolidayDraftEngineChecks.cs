namespace Acme.Workforce.Absence.Tests.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batched holiday schedule code, run by the project's runner.</summary>
public static class HolidayDraftEngineChecks
{
    public static void HolidayDraftEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayDraftEngine.DefaultHolidayDraftEngine(Flags.NewRegistry());
        var item = HolidayDraft.NewHolidayDraft("code-181", "title-41");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayDraftEngineKeepsResultsWithinBounds();
    }
}
