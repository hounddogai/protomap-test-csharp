namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LeaveRequestTargetReportChecks
{
    public static void LeaveRequestTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705109927);
        var items = new List<LeaveRequestTarget>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LeaveRequestTarget.NewLeaveRequestTarget($"leave-request-target-{index}", "sample"));
        }
        var report = LeaveRequestTargetReport.BuildLeaveRequestTargetReport("test", items, clock);
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
        LeaveRequestTargetReportCountsEachRecordOnce();
    }
}
