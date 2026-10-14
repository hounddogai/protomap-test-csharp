namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSchedule;

using Acme.Workforce.Contractors.Vendor.VendorSchedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of vendor schedule code, run by the project's runner.</summary>
public static class RollingVendorQuotaReportChecks
{
    public static void RollingVendorQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703011969);
        var items = new List<RollingVendorQuota>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RollingVendorQuota.NewRollingVendorQuota($"rolling-vendor-quota-{index}", "sample"));
        }
        var report = RollingVendorQuotaReport.BuildRollingVendorQuotaReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RollingVendorQuotaReportCountsEachRecordOnce();
    }
}
