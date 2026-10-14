namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentNoticeReportChecks
{
    public static void AdjustmentNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702354760);
        var items = new List<AdjustmentNotice>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AdjustmentNotice.NewAdjustmentNotice($"adjustment-notice-{index}", "sample"));
        }
        var report = AdjustmentNoticeReport.BuildAdjustmentNoticeReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AdjustmentNoticeReportCountsEachRecordOnce();
    }
}
