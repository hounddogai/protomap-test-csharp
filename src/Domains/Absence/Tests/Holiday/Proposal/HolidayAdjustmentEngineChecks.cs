namespace Acme.Workforce.Absence.Tests.Holiday.Proposal;

using Acme.Workforce.Absence.Holiday.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HolidayAdjustmentEngineChecks
{
    public static void HolidayAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayAdjustmentEngine.DefaultHolidayAdjustmentEngine();
        var item = HolidayAdjustment.NewHolidayAdjustment("code-164", "label-458");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayAdjustmentEngineKeepsResultsWithinBounds();
    }
}
