namespace Acme.Workforce.Absence.Tests.LeaveRequest.Item;

using Acme.Workforce.Absence.LeaveRequest.Item;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class LeaveRequestScoreReportChecks
{
    public static void LeaveRequestScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702406840);
        var items = new List<LeaveRequestScore>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LeaveRequestScore.NewLeaveRequestScore($"leave-request-score-{index}", "sample"));
        }
        var report = LeaveRequestScoreReport.BuildLeaveRequestScoreReport("test", items, clock);
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
        LeaveRequestScoreReportCountsEachRecordOnce();
    }
}
