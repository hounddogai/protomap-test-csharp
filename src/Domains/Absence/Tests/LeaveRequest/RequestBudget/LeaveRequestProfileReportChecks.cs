namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestBudget;

using Acme.Workforce.Absence.LeaveRequest.RequestBudget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request budget code, run by the project's runner.</summary>
public static class LeaveRequestProfileReportChecks
{
    public static void LeaveRequestProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709243810);
        var items = new List<LeaveRequestProfile>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LeaveRequestProfile.NewLeaveRequestProfile($"leave-request-profile-{index}", "sample"));
        }
        var report = LeaveRequestProfileReport.BuildLeaveRequestProfileReport("test", items, clock);
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
        LeaveRequestProfileReportCountsEachRecordOnce();
    }
}
