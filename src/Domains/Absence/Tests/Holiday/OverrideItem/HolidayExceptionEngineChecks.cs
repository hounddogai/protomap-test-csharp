namespace Acme.Workforce.Absence.Tests.Holiday.OverrideItem;

using Acme.Workforce.Absence.Holiday.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HolidayExceptionEngineChecks
{
    public static void HolidayExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayExceptionEngine.DefaultHolidayExceptionEngine();
        var item = HolidayException.NewHolidayException("code-672", "title-760");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayExceptionEngineKeepsResultsWithinBounds();
    }
}
