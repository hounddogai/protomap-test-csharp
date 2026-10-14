namespace Acme.Workforce.Absence.Tests.LeaveRequest.Baseline;

using Acme.Workforce.Absence.LeaveRequest.Baseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LeaveRequestOverrideReportChecks
{
    public static void LeaveRequestOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705242421);
        var items = new List<LeaveRequestOverride>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LeaveRequestOverride.NewLeaveRequestOverride($"leave-request-override-{index}", "sample"));
        }
        var report = LeaveRequestOverrideReport.BuildLeaveRequestOverrideReport("test", items, clock);
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
        LeaveRequestOverrideReportCountsEachRecordOnce();
    }
}
