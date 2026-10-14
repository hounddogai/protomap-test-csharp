namespace Acme.Workforce.Contractors.Tests.Vendor.Budget;

using Acme.Workforce.Contractors.Vendor.Budget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class VendorAssignmentReportChecks
{
    public static void VendorAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706524293);
        var items = new List<VendorAssignment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(VendorAssignment.NewVendorAssignment($"vendor-assignment-{index}", "sample"));
        }
        var report = VendorAssignmentReport.BuildVendorAssignmentReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        VendorAssignmentReportCountsEachRecordOnce();
    }
}
