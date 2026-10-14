namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LaptopEstimateReportChecks
{
    public static void LaptopEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700811638);
        var items = new List<LaptopEstimate>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LaptopEstimate.NewLaptopEstimate($"laptop-estimate-{index}", "sample"));
        }
        var report = LaptopEstimateReport.BuildLaptopEstimateReport("test", items, clock);
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
        LaptopEstimateReportCountsEachRecordOnce();
    }
}
