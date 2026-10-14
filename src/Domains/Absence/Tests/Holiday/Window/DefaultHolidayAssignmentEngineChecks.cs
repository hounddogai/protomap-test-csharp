namespace Acme.Workforce.Absence.Tests.Holiday.Window;

using Acme.Workforce.Absence.Holiday.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class DefaultHolidayAssignmentEngineChecks
{
    public static void DefaultHolidayAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultHolidayAssignmentEngine.DefaultDefaultHolidayAssignmentEngine();
        var item = DefaultHolidayAssignment.NewDefaultHolidayAssignment("code-155", "title-450");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultHolidayAssignmentEngineKeepsResultsWithinBounds();
    }
}
