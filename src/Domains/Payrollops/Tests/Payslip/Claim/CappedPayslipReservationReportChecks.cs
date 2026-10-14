namespace Acme.Workforce.Payrollops.Tests.Payslip.Claim;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class CappedPayslipReservationReportChecks
{
    public static void CappedPayslipReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707088580);
        var items = new List<CappedPayslipReservation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CappedPayslipReservation.NewCappedPayslipReservation($"capped-payslip-reservation-{index}", "sample"));
        }
        var report = CappedPayslipReservationReport.BuildCappedPayslipReservationReport("test", items, clock);
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
        CappedPayslipReservationReportCountsEachRecordOnce();
    }
}
