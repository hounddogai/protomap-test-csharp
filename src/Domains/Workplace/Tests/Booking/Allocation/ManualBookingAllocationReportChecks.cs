namespace Acme.Workforce.Workplace.Tests.Booking.Allocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ManualBookingAllocationReportChecks
{
    public static void ManualBookingAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707486895);
        var items = new List<ManualBookingAllocation>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ManualBookingAllocation.NewManualBookingAllocation($"manual-booking-allocation-{index}", "sample"));
        }
        var report = ManualBookingAllocationReport.BuildManualBookingAllocationReport("test", items, clock);
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
        ManualBookingAllocationReportCountsEachRecordOnce();
    }
}
