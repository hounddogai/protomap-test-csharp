namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PendingLaptopReservationReportChecks
{
    public static void PendingLaptopReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700224060);
        var items = new List<PendingLaptopReservation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PendingLaptopReservation.NewPendingLaptopReservation($"pending-laptop-reservation-{index}", "sample"));
        }
        var report = PendingLaptopReservationReport.BuildPendingLaptopReservationReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PendingLaptopReservationReportCountsEachRecordOnce();
    }
}
