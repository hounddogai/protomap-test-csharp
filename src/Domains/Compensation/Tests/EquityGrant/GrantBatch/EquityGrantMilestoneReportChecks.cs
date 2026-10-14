namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantBatch;

using Acme.Workforce.Compensation.EquityGrant.GrantBatch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of grant batch code, run by the project's runner.</summary>
public static class EquityGrantMilestoneReportChecks
{
    public static void EquityGrantMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702350856);
        var items = new List<EquityGrantMilestone>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(EquityGrantMilestone.NewEquityGrantMilestone($"equity-grant-milestone-{index}", "sample"));
        }
        var report = EquityGrantMilestoneReport.BuildEquityGrantMilestoneReport("test", items, clock);
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
        EquityGrantMilestoneReportCountsEachRecordOnce();
    }
}
