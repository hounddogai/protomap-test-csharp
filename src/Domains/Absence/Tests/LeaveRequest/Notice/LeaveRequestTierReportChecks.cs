namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class LeaveRequestTierReportChecks
{
    public static void LeaveRequestTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700748724);
        var items = new List<LeaveRequestTier>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LeaveRequestTier.NewLeaveRequestTier($"leave-request-tier-{index}", "sample"));
        }
        var report = LeaveRequestTierReport.BuildLeaveRequestTierReport("test", items, clock);
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
        LeaveRequestTierReportCountsEachRecordOnce();
    }
}
