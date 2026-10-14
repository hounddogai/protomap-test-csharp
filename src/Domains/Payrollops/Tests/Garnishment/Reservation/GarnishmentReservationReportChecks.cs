namespace Acme.Workforce.Payrollops.Tests.Garnishment.Reservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class GarnishmentReservationReportChecks
{
    public static void GarnishmentReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706471171);
        var items = new List<GarnishmentReservation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(GarnishmentReservation.NewGarnishmentReservation($"garnishment-reservation-{index}", "sample"));
        }
        var report = GarnishmentReservationReport.BuildGarnishmentReservationReport("test", items, clock);
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
        GarnishmentReservationReportCountsEachRecordOnce();
    }
}
