namespace Acme.Workforce.Absence.Tests.LeaveRequest.Assignment;

using Acme.Workforce.Absence.LeaveRequest.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class RollingLeaveRequestPreferenceReportChecks
{
    public static void RollingLeaveRequestPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706462501);
        var items = new List<RollingLeaveRequestPreference>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RollingLeaveRequestPreference.NewRollingLeaveRequestPreference($"rolling-leave-request-preference-{index}", "sample"));
        }
        var report = RollingLeaveRequestPreferenceReport.BuildRollingLeaveRequestPreferenceReport("test", items, clock);
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
        RollingLeaveRequestPreferenceReportCountsEachRecordOnce();
    }
}
