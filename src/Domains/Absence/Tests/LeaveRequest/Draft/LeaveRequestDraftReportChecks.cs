namespace Acme.Workforce.Absence.Tests.LeaveRequest.Draft;

using Acme.Workforce.Absence.LeaveRequest.Draft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LeaveRequestDraftReportChecks
{
    public static void LeaveRequestDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703071853);
        var items = new List<LeaveRequestDraft>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LeaveRequestDraft.NewLeaveRequestDraft($"leave-request-draft-{index}", "sample"));
        }
        var report = LeaveRequestDraftReport.BuildLeaveRequestDraftReport("test", items, clock);
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
        LeaveRequestDraftReportCountsEachRecordOnce();
    }
}
