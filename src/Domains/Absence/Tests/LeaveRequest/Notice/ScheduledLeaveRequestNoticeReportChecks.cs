namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestNoticeReportChecks
{
    public static void ScheduledLeaveRequestNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701685692);
        var items = new List<ScheduledLeaveRequestNotice>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScheduledLeaveRequestNotice.NewScheduledLeaveRequestNotice($"scheduled-leave-request-notice-{index}", "sample"));
        }
        var report = ScheduledLeaveRequestNoticeReport.BuildScheduledLeaveRequestNoticeReport("test", items, clock);
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
        ScheduledLeaveRequestNoticeReportCountsEachRecordOnce();
    }
}
