namespace Acme.Workforce.Workplace.Tests.Occupancy.Reservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class OccupancyReservationReportChecks
{
    public static void OccupancyReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703747677);
        var items = new List<OccupancyReservation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(OccupancyReservation.NewOccupancyReservation($"occupancy-reservation-{index}", "sample"));
        }
        var report = OccupancyReservationReport.BuildOccupancyReservationReport("test", items, clock);
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
        OccupancyReservationReportCountsEachRecordOnce();
    }
}
