namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class LeaveRequestGroupReportChecks
{
    public static void LeaveRequestGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709580847);
        var items = new List<LeaveRequestGroup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LeaveRequestGroup.NewLeaveRequestGroup($"leave-request-group-{index}", "sample"));
        }
        var report = LeaveRequestGroupReport.BuildLeaveRequestGroupReport("test", items, clock);
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
        LeaveRequestGroupReportCountsEachRecordOnce();
    }
}
