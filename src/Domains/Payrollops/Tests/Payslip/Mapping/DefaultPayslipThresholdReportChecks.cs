namespace Acme.Workforce.Payrollops.Tests.Payslip.Mapping;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultPayslipThresholdReportChecks
{
    public static void DefaultPayslipThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700743123);
        var items = new List<DefaultPayslipThreshold>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(DefaultPayslipThreshold.NewDefaultPayslipThreshold($"default-payslip-threshold-{index}", "sample"));
        }
        var report = DefaultPayslipThresholdReport.BuildDefaultPayslipThresholdReport("test", items, clock);
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
        DefaultPayslipThresholdReportCountsEachRecordOnce();
    }
}
