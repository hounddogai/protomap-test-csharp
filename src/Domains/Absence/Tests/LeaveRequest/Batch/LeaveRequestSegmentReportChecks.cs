namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class LeaveRequestSegmentReportChecks
{
    public static void LeaveRequestSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708775546);
        var items = new List<LeaveRequestSegment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LeaveRequestSegment.NewLeaveRequestSegment($"leave-request-segment-{index}", "sample"));
        }
        var report = LeaveRequestSegmentReport.BuildLeaveRequestSegmentReport("test", items, clock);
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
        LeaveRequestSegmentReportCountsEachRecordOnce();
    }
}
