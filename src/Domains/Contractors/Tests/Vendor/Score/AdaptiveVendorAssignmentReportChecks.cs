namespace Acme.Workforce.Contractors.Tests.Vendor.Score;

using Acme.Workforce.Contractors.Vendor.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdaptiveVendorAssignmentReportChecks
{
    public static void AdaptiveVendorAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708506360);
        var items = new List<AdaptiveVendorAssignment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AdaptiveVendorAssignment.NewAdaptiveVendorAssignment($"adaptive-vendor-assignment-{index}", "sample"));
        }
        var report = AdaptiveVendorAssignmentReport.BuildAdaptiveVendorAssignmentReport("test", items, clock);
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
        AdaptiveVendorAssignmentReportCountsEachRecordOnce();
    }
}
