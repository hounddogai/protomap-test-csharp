namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSchedule;

using Acme.Workforce.Contractors.Vendor.VendorSchedule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of vendor schedule code, run by the project's runner.</summary>
public static class TieredVendorScheduleEngineChecks
{
    public static void TieredVendorScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = TieredVendorScheduleEngine.DefaultTieredVendorScheduleEngine(Flags.NewRegistry());
        var item = TieredVendorSchedule.NewTieredVendorSchedule("code-369", "label-132");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredVendorScheduleEngineKeepsResultsWithinBounds();
    }
}
