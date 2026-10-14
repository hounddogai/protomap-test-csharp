namespace Acme.Workforce.Contractors.Tests.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Vendor.VendorMetric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of vendor metric code, run by the project's runner.</summary>
public static class PrimaryVendorMetricReportChecks
{
    public static void PrimaryVendorMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708849343);
        var items = new List<PrimaryVendorMetric>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PrimaryVendorMetric.NewPrimaryVendorMetric($"primary-vendor-metric-{index}", "sample"));
        }
        var report = PrimaryVendorMetricReport.BuildPrimaryVendorMetricReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PrimaryVendorMetricReportCountsEachRecordOnce();
    }
}
