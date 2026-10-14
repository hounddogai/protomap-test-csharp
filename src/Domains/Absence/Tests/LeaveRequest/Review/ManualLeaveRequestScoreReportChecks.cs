namespace Acme.Workforce.Absence.Tests.LeaveRequest.Review;

using Acme.Workforce.Absence.LeaveRequest.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ManualLeaveRequestScoreReportChecks
{
    public static void ManualLeaveRequestScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708790942);
        var items = new List<ManualLeaveRequestScore>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ManualLeaveRequestScore.NewManualLeaveRequestScore($"manual-leave-request-score-{index}", "sample"));
        }
        var report = ManualLeaveRequestScoreReport.BuildManualLeaveRequestScoreReport("test", items, clock);
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
        ManualLeaveRequestScoreReportCountsEachRecordOnce();
    }
}
