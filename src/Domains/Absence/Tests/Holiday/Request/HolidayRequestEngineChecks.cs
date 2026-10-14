namespace Acme.Workforce.Absence.Tests.Holiday.Request;

using Acme.Workforce.Absence.Holiday.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class HolidayRequestEngineChecks
{
    public static void HolidayRequestEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayRequestEngine.DefaultHolidayRequestEngine();
        var item = HolidayRequest.NewHolidayRequest("code-964", "name-882");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayRequestEngineKeepsResultsWithinBounds();
    }
}
