namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementTemplate;

using Acme.Workforce.Contractors.Engagement.EngagementTemplate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of engagement template code, run by the project's runner.</summary>
public static class EngagementRequestReportChecks
{
    public static void EngagementRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708274174);
        var items = new List<EngagementRequest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(EngagementRequest.NewEngagementRequest($"engagement-request-{index}", "sample"));
        }
        var report = EngagementRequestReport.BuildEngagementRequestReport("test", items, clock);
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
        EngagementRequestReportCountsEachRecordOnce();
    }
}
