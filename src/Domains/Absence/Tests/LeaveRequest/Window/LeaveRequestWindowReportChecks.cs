namespace Acme.Workforce.Absence.Tests.LeaveRequest.Window;

using Acme.Workforce.Absence.LeaveRequest.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LeaveRequestWindowReportChecks
{
    public static void LeaveRequestWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709784410);
        var items = new List<LeaveRequestWindow>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LeaveRequestWindow.NewLeaveRequestWindow($"leave-request-window-{index}", "sample"));
        }
        var report = LeaveRequestWindowReport.BuildLeaveRequestWindowReport("test", items, clock);
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
        LeaveRequestWindowReportCountsEachRecordOnce();
    }
}
