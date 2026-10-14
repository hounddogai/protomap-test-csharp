namespace Acme.Workforce.Workplace.Tests.AccessBadge.Estimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class GlobalAccessBadgeEstimateReportChecks
{
    public static void GlobalAccessBadgeEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708556967);
        var items = new List<GlobalAccessBadgeEstimate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(GlobalAccessBadgeEstimate.NewGlobalAccessBadgeEstimate($"global-access-badge-estimate-{index}", "sample"));
        }
        var report = GlobalAccessBadgeEstimateReport.BuildGlobalAccessBadgeEstimateReport("test", items, clock);
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
        GlobalAccessBadgeEstimateReportCountsEachRecordOnce();
    }
}
