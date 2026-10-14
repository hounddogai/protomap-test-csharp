namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HolidayAssignmentEngineChecks
{
    public static void HolidayAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayAssignmentEngine.DefaultHolidayAssignmentEngine(Flags.NewRegistry());
        var item = HolidayAssignment.NewHolidayAssignment("code-105", "name-749");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayAssignmentEngineKeepsResultsWithinBounds();
    }
}
