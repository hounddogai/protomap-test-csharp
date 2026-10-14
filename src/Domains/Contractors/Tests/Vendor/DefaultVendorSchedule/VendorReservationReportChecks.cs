namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class VendorReservationReportChecks
{
    public static void VendorReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703861224);
        var items = new List<VendorReservation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(VendorReservation.NewVendorReservation($"vendor-reservation-{index}", "sample"));
        }
        var report = VendorReservationReport.BuildVendorReservationReport("test", items, clock);
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
        VendorReservationReportCountsEachRecordOnce();
    }
}
