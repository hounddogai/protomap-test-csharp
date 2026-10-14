namespace Acme.Workforce.Payrollops.Tests.Payslip.Snapshot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class PayslipSnapshotReportChecks
{
    public static void PayslipSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701931741);
        var items = new List<PayslipSnapshot>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PayslipSnapshot.NewPayslipSnapshot($"payslip-snapshot-{index}", "sample"));
        }
        var report = PayslipSnapshotReport.BuildPayslipSnapshotReport("test", items, clock);
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
        PayslipSnapshotReportCountsEachRecordOnce();
    }
}
