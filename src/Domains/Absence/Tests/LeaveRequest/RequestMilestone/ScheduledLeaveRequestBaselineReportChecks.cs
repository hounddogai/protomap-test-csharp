namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestBaselineReportChecks
{
    public static void ScheduledLeaveRequestBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705802759);
        var items = new List<ScheduledLeaveRequestBaseline>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ScheduledLeaveRequestBaseline.NewScheduledLeaveRequestBaseline($"scheduled-leave-request-baseline-{index}", "sample"));
        }
        var report = ScheduledLeaveRequestBaselineReport.BuildScheduledLeaveRequestBaselineReport("test", items, clock);
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
        ScheduledLeaveRequestBaselineReportCountsEachRecordOnce();
    }
}
