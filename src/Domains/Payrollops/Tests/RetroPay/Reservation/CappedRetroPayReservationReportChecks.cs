namespace Acme.Workforce.Payrollops.Tests.RetroPay.Reservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedRetroPayReservationReportChecks
{
    public static void CappedRetroPayReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704691324);
        var items = new List<CappedRetroPayReservation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(CappedRetroPayReservation.NewCappedRetroPayReservation($"capped-retro-pay-reservation-{index}", "sample"));
        }
        var report = CappedRetroPayReservationReport.BuildCappedRetroPayReservationReport("test", items, clock);
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
        CappedRetroPayReservationReportCountsEachRecordOnce();
    }
}
