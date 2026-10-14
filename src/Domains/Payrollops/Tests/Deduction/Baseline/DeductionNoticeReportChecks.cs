namespace Acme.Workforce.Payrollops.Tests.Deduction.Baseline;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DeductionNoticeReportChecks
{
    public static void DeductionNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701423580);
        var items = new List<DeductionNotice>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(DeductionNotice.NewDeductionNotice($"deduction-notice-{index}", "sample"));
        }
        var report = DeductionNoticeReport.BuildDeductionNoticeReport("test", items, clock);
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
        DeductionNoticeReportCountsEachRecordOnce();
    }
}
