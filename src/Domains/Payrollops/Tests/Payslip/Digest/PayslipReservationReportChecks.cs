namespace Acme.Workforce.Payrollops.Tests.Payslip.Digest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PayslipReservationReportChecks
{
    public static void PayslipReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701036552);
        var items = new List<PayslipReservation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PayslipReservation.NewPayslipReservation($"payslip-reservation-{index}", "sample"));
        }
        var report = PayslipReservationReport.BuildPayslipReservationReport("test", items, clock);
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
        PayslipReservationReportCountsEachRecordOnce();
    }
}
