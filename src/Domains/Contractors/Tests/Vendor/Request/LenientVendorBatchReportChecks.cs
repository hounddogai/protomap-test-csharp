namespace Acme.Workforce.Contractors.Tests.Vendor.Request;

using Acme.Workforce.Contractors.Vendor.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LenientVendorBatchReportChecks
{
    public static void LenientVendorBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709012614);
        var items = new List<LenientVendorBatch>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LenientVendorBatch.NewLenientVendorBatch($"lenient-vendor-batch-{index}", "sample"));
        }
        var report = LenientVendorBatchReport.BuildLenientVendorBatchReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LenientVendorBatchReportCountsEachRecordOnce();
    }
}
