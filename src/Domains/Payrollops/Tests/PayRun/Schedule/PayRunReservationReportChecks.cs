namespace Acme.Workforce.Payrollops.Tests.PayRun.Schedule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PayRunReservationReportChecks
{
    public static void PayRunReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701653336);
        var items = new List<PayRunReservation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PayRunReservation.NewPayRunReservation($"pay-run-reservation-{index}", "sample"));
        }
        var report = PayRunReservationReport.BuildPayRunReservationReport("test", items, clock);
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
        PayRunReservationReportCountsEachRecordOnce();
    }
}
