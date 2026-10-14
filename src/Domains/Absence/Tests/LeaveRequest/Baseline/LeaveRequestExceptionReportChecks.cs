namespace Acme.Workforce.Absence.Tests.LeaveRequest.Baseline;

using Acme.Workforce.Absence.LeaveRequest.Baseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LeaveRequestExceptionReportChecks
{
    public static void LeaveRequestExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704882915);
        var items = new List<LeaveRequestException>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LeaveRequestException.NewLeaveRequestException($"leave-request-exception-{index}", "sample"));
        }
        var report = LeaveRequestExceptionReport.BuildLeaveRequestExceptionReport("test", items, clock);
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
        LeaveRequestExceptionReportCountsEachRecordOnce();
    }
}
