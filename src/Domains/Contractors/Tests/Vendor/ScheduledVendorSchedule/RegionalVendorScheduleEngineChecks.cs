namespace Acme.Workforce.Contractors.Tests.Vendor.ScheduledVendorSchedule;

using Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

/// <summary>Checks of scheduled vendor schedule code, run by the project's runner.</summary>
public static class RegionalVendorScheduleEngineChecks
{
    public static void RegionalVendorScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalVendorScheduleEngine.DefaultRegionalVendorScheduleEngine();
        var item = RegionalVendorSchedule.NewRegionalVendorSchedule("code-919", "title-847");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalVendorScheduleEngineKeepsResultsWithinBounds();
    }
}
