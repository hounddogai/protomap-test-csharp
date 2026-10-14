namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualNoticeReportChecks
{
    public static void AccrualNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702641196);
        var items = new List<AccrualNotice>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AccrualNotice.NewAccrualNotice($"accrual-notice-{index}", "sample"));
        }
        var report = AccrualNoticeReport.BuildAccrualNoticeReport("test", items, clock);
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
        AccrualNoticeReportCountsEachRecordOnce();
    }
}
