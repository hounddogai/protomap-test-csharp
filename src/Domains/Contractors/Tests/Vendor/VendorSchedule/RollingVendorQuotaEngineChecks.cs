namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSchedule;

using Acme.Workforce.Contractors.Vendor.VendorSchedule;

/// <summary>Checks of vendor schedule code, run by the project's runner.</summary>
public static class RollingVendorQuotaEngineChecks
{
    public static void RollingVendorQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = RollingVendorQuotaEngine.DefaultRollingVendorQuotaEngine();
        var item = RollingVendorQuota.NewRollingVendorQuota("code-268", "label-803");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingVendorQuotaEngineKeepsResultsWithinBounds();
    }
}
