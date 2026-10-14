namespace Acme.Workforce.Assets.Tests.LicenseSeat.Reservation;

using Acme.Workforce.Assets.LicenseSeat.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LicenseSeatReservationReportChecks
{
    public static void LicenseSeatReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703563460);
        var items = new List<LicenseSeatReservation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LicenseSeatReservation.NewLicenseSeatReservation($"license-seat-reservation-{index}", "sample"));
        }
        var report = LicenseSeatReservationReport.BuildLicenseSeatReservationReport("test", items, clock);
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
        LicenseSeatReservationReportCountsEachRecordOnce();
    }
}
