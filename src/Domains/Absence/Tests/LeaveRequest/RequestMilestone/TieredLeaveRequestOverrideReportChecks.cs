namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class TieredLeaveRequestOverrideReportChecks
{
    public static void TieredLeaveRequestOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701491644);
        var items = new List<TieredLeaveRequestOverride>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TieredLeaveRequestOverride.NewTieredLeaveRequestOverride($"tiered-leave-request-override-{index}", "sample"));
        }
        var report = TieredLeaveRequestOverrideReport.BuildTieredLeaveRequestOverrideReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        TieredLeaveRequestOverrideReportCountsEachRecordOnce();
    }
}
