namespace Acme.Workforce.Contractors.Tests.Rate.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class WeightedRateRequestReportChecks
{
    public static void WeightedRateRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703090542);
        var items = new List<WeightedRateRequest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WeightedRateRequest.NewWeightedRateRequest($"weighted-rate-request-{index}", "sample"));
        }
        var report = WeightedRateRequestReport.BuildWeightedRateRequestReport("test", items, clock);
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
        WeightedRateRequestReportCountsEachRecordOnce();
    }
}
