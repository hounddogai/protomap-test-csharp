namespace Acme.Workforce.Contractors.Tests.Engagement.Checkpoint;

using Acme.Workforce.Contractors.Engagement.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalEngagementOverrideReportChecks
{
    public static void RegionalEngagementOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706907039);
        var items = new List<RegionalEngagementOverride>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RegionalEngagementOverride.NewRegionalEngagementOverride($"regional-engagement-override-{index}", "sample"));
        }
        var report = RegionalEngagementOverrideReport.BuildRegionalEngagementOverrideReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RegionalEngagementOverrideReportCountsEachRecordOnce();
    }
}
