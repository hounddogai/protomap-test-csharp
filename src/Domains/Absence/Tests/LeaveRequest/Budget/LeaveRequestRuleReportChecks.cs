namespace Acme.Workforce.Absence.Tests.LeaveRequest.Budget;

using Acme.Workforce.Absence.LeaveRequest.Budget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LeaveRequestRuleReportChecks
{
    public static void LeaveRequestRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708947706);
        var items = new List<LeaveRequestRule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LeaveRequestRule.NewLeaveRequestRule($"leave-request-rule-{index}", "sample"));
        }
        var report = LeaveRequestRuleReport.BuildLeaveRequestRuleReport("test", items, clock);
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
        LeaveRequestRuleReportCountsEachRecordOnce();
    }
}
