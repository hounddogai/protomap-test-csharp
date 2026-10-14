namespace Acme.Workforce.Absence.Tests.LeaveRequest.Proposal;

using Acme.Workforce.Absence.LeaveRequest.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RegionalLeaveRequestItemReportChecks
{
    public static void RegionalLeaveRequestItemReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704939809);
        var items = new List<RegionalLeaveRequestItem>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RegionalLeaveRequestItem.NewRegionalLeaveRequestItem($"regional-leave-request-item-{index}", "sample"));
        }
        var report = RegionalLeaveRequestItemReport.BuildRegionalLeaveRequestItemReport("test", items, clock);
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
        RegionalLeaveRequestItemReportCountsEachRecordOnce();
    }
}
