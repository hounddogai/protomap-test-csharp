namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class RegionalPayslipReservationReportChecks
{
    public static void RegionalPayslipReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707817397);
        var items = new List<RegionalPayslipReservation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RegionalPayslipReservation.NewRegionalPayslipReservation($"regional-payslip-reservation-{index}", "sample"));
        }
        var report = RegionalPayslipReservationReport.BuildRegionalPayslipReservationReport("test", items, clock);
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
        RegionalPayslipReservationReportCountsEachRecordOnce();
    }
}
