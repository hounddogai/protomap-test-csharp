namespace Acme.Workforce.Absence.Tests.LeaveRequest.Assignment;

using Acme.Workforce.Absence.LeaveRequest.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LeaveRequestBundleReportChecks
{
    public static void LeaveRequestBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702694502);
        var items = new List<LeaveRequestBundle>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LeaveRequestBundle.NewLeaveRequestBundle($"leave-request-bundle-{index}", "sample"));
        }
        var report = LeaveRequestBundleReport.BuildLeaveRequestBundleReport("test", items, clock);
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
        LeaveRequestBundleReportCountsEachRecordOnce();
    }
}
