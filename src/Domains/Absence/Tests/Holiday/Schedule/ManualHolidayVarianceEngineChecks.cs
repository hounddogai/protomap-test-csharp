namespace Acme.Workforce.Absence.Tests.Holiday.Schedule;

using Acme.Workforce.Absence.Holiday.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ManualHolidayVarianceEngineChecks
{
    public static void ManualHolidayVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = ManualHolidayVarianceEngine.DefaultManualHolidayVarianceEngine();
        var item = ManualHolidayVariance.NewManualHolidayVariance("code-884", "name-510");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualHolidayVarianceEngineKeepsResultsWithinBounds();
    }
}
