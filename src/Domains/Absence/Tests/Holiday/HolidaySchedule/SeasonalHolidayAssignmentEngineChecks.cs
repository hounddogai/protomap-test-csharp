namespace Acme.Workforce.Absence.Tests.Holiday.HolidaySchedule;

using Acme.Workforce.Absence.Holiday.HolidaySchedule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of holiday schedule code, run by the project's runner.</summary>
public static class SeasonalHolidayAssignmentEngineChecks
{
    public static void SeasonalHolidayAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalHolidayAssignmentEngine.DefaultSeasonalHolidayAssignmentEngine(Flags.NewRegistry());
        var item = SeasonalHolidayAssignment.NewSeasonalHolidayAssignment("code-774", "label-444");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalHolidayAssignmentEngineKeepsResultsWithinBounds();
    }
}
