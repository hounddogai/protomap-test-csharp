namespace Acme.Workforce.Talent.Tests.Mobility.Reservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MobilityReservationReportChecks
{
    public static void MobilityReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703994735);
        var items = new List<MobilityReservation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(MobilityReservation.NewMobilityReservation($"mobility-reservation-{index}", "sample"));
        }
        var report = MobilityReservationReport.BuildMobilityReservationReport("test", items, clock);
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
        MobilityReservationReportCountsEachRecordOnce();
    }
}
