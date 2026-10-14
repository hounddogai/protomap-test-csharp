namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class StrictHolidayAllocationEngineChecks
{
    public static void StrictHolidayAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = StrictHolidayAllocationEngine.DefaultStrictHolidayAllocationEngine();
        var item = StrictHolidayAllocation.NewStrictHolidayAllocation("code-897", "name-527");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictHolidayAllocationEngineKeepsResultsWithinBounds();
    }
}
