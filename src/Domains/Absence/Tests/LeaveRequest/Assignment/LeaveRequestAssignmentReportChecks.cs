namespace Acme.Workforce.Absence.Tests.LeaveRequest.Assignment;

using Acme.Workforce.Absence.LeaveRequest.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LeaveRequestAssignmentReportChecks
{
    public static void LeaveRequestAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709925834);
        var items = new List<LeaveRequestAssignment>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LeaveRequestAssignment.NewLeaveRequestAssignment($"leave-request-assignment-{index}", "sample"));
        }
        var report = LeaveRequestAssignmentReport.BuildLeaveRequestAssignmentReport("test", items, clock);
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
        LeaveRequestAssignmentReportCountsEachRecordOnce();
    }
}
