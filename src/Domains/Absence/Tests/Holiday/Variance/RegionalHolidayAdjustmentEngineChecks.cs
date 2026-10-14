namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RegionalHolidayAdjustmentEngineChecks
{
    public static void RegionalHolidayAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalHolidayAdjustmentEngine.DefaultRegionalHolidayAdjustmentEngine(Flags.NewRegistry());
        var item = RegionalHolidayAdjustment.NewRegionalHolidayAdjustment("code-458", "title-330");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalHolidayAdjustmentEngineKeepsResultsWithinBounds();
    }
}
