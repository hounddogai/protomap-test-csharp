namespace Acme.Workforce.Absence.Tests.LeaveRequest.Reservation;

using Acme.Workforce.Absence.LeaveRequest.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LeaveRequestNoticeReportChecks
{
    public static void LeaveRequestNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703550218);
        var items = new List<LeaveRequestNotice>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LeaveRequestNotice.NewLeaveRequestNotice($"leave-request-notice-{index}", "sample"));
        }
        var report = LeaveRequestNoticeReport.BuildLeaveRequestNoticeReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LeaveRequestNoticeReportCountsEachRecordOnce();
    }
}
