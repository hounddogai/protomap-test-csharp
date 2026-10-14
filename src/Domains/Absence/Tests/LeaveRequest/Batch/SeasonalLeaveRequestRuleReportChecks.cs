namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SeasonalLeaveRequestRuleReportChecks
{
    public static void SeasonalLeaveRequestRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705035329);
        var items = new List<SeasonalLeaveRequestRule>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SeasonalLeaveRequestRule.NewSeasonalLeaveRequestRule($"seasonal-leave-request-rule-{index}", "sample"));
        }
        var report = SeasonalLeaveRequestRuleReport.BuildSeasonalLeaveRequestRuleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SeasonalLeaveRequestRuleReportCountsEachRecordOnce();
    }
}
