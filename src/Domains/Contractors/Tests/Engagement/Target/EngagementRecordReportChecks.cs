namespace Acme.Workforce.Contractors.Tests.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Target;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class EngagementRecordReportChecks
{
    public static void EngagementRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704039380);
        var items = new List<EngagementRecord>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(EngagementRecord.NewEngagementRecord($"engagement-record-{index}", "sample"));
        }
        var report = EngagementRecordReport.BuildEngagementRecordReport("test", items, clock);
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
        EngagementRecordReportCountsEachRecordOnce();
    }
}
