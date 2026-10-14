namespace Acme.Workforce.Absence.Tests.Holiday.Allocation;

using Acme.Workforce.Absence.Holiday.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class HolidayAllocationEngineChecks
{
    public static void HolidayAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayAllocationEngine.DefaultHolidayAllocationEngine();
        var item = HolidayAllocation.NewHolidayAllocation("code-320", "name-777");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayAllocationEngineKeepsResultsWithinBounds();
    }
}
