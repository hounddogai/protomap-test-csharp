namespace Acme.Workforce.Payrollops.Tests.Deduction.Review;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ManualDeductionNoticeReportChecks
{
    public static void ManualDeductionNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706565372);
        var items = new List<ManualDeductionNotice>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ManualDeductionNotice.NewManualDeductionNotice($"manual-deduction-notice-{index}", "sample"));
        }
        var report = ManualDeductionNoticeReport.BuildManualDeductionNoticeReport("test", items, clock);
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
        ManualDeductionNoticeReportCountsEachRecordOnce();
    }
}
