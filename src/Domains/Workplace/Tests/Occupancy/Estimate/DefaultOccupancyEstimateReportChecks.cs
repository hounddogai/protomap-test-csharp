namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class DefaultOccupancyEstimateReportChecks
{
    public static void DefaultOccupancyEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700923708);
        var items = new List<DefaultOccupancyEstimate>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(DefaultOccupancyEstimate.NewDefaultOccupancyEstimate($"default-occupancy-estimate-{index}", "sample"));
        }
        var report = DefaultOccupancyEstimateReport.BuildDefaultOccupancyEstimateReport("test", items, clock);
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
        DefaultOccupancyEstimateReportCountsEachRecordOnce();
    }
}
