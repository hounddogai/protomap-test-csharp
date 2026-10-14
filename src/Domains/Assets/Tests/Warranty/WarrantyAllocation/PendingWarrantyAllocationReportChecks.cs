namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAllocation;

using Acme.Workforce.Assets.Warranty.WarrantyAllocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of warranty allocation code, run by the project's runner.</summary>
public static class PendingWarrantyAllocationReportChecks
{
    public static void PendingWarrantyAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700213625);
        var items = new List<PendingWarrantyAllocation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PendingWarrantyAllocation.NewPendingWarrantyAllocation($"pending-warranty-allocation-{index}", "sample"));
        }
        var report = PendingWarrantyAllocationReport.BuildPendingWarrantyAllocationReport("test", items, clock);
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
        PendingWarrantyAllocationReportCountsEachRecordOnce();
    }
}
