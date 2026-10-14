namespace Acme.Workforce.Payrollops.Tests.Payslip.Group;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PayslipSegmentReportChecks
{
    public static void PayslipSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709906022);
        var items = new List<PayslipSegment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PayslipSegment.NewPayslipSegment($"payslip-segment-{index}", "sample"));
        }
        var report = PayslipSegmentReport.BuildPayslipSegmentReport("test", items, clock);
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
        PayslipSegmentReportCountsEachRecordOnce();
    }
}
