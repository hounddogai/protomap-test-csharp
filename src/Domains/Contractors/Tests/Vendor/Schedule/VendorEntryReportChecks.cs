namespace Acme.Workforce.Contractors.Tests.Vendor.Schedule;

using Acme.Workforce.Contractors.Vendor.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class VendorEntryReportChecks
{
    public static void VendorEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706103740);
        var items = new List<VendorEntry>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(VendorEntry.NewVendorEntry($"vendor-entry-{index}", "sample"));
        }
        var report = VendorEntryReport.BuildVendorEntryReport("test", items, clock);
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
        VendorEntryReportCountsEachRecordOnce();
    }
}
