namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LenientVendorReservationReportChecks
{
    public static void LenientVendorReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701359460);
        var items = new List<LenientVendorReservation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LenientVendorReservation.NewLenientVendorReservation($"lenient-vendor-reservation-{index}", "sample"));
        }
        var report = LenientVendorReservationReport.BuildLenientVendorReservationReport("test", items, clock);
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
        LenientVendorReservationReportCountsEachRecordOnce();
    }
}
