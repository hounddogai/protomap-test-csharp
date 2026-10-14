namespace Acme.Workforce.Absence.Tests.LeaveRequest.Schedule;

using Acme.Workforce.Absence.LeaveRequest.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LeaveRequestScheduleReportChecks
{
    public static void LeaveRequestScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708360246);
        var items = new List<LeaveRequestSchedule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LeaveRequestSchedule.NewLeaveRequestSchedule($"leave-request-schedule-{index}", "sample"));
        }
        var report = LeaveRequestScheduleReport.BuildLeaveRequestScheduleReport("test", items, clock);
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
        LeaveRequestScheduleReportCountsEachRecordOnce();
    }
}
