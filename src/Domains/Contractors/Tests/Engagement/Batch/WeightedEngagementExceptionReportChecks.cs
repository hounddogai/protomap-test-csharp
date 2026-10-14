namespace Acme.Workforce.Contractors.Tests.Engagement.Batch;

using Acme.Workforce.Contractors.Engagement.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedEngagementExceptionReportChecks
{
    public static void WeightedEngagementExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703611327);
        var items = new List<WeightedEngagementException>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WeightedEngagementException.NewWeightedEngagementException($"weighted-engagement-exception-{index}", "sample"));
        }
        var report = WeightedEngagementExceptionReport.BuildWeightedEngagementExceptionReport("test", items, clock);
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
        WeightedEngagementExceptionReportCountsEachRecordOnce();
    }
}
