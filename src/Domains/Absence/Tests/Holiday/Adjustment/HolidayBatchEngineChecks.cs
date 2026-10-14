namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HolidayBatchEngineChecks
{
    public static void HolidayBatchEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayBatchEngine.DefaultHolidayBatchEngine();
        var item = HolidayBatch.NewHolidayBatch("code-731", "title-642");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayBatchEngineKeepsResultsWithinBounds();
    }
}
