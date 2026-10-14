namespace Acme.Workforce.Talent.Tests.CareerPath.PathDigest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>Checks of path digest code, run by the project's runner.</summary>
public static class CareerPathEstimateReportChecks
{
    public static void CareerPathEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704303227);
        var items = new List<CareerPathEstimate>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CareerPathEstimate.NewCareerPathEstimate($"career-path-estimate-{index}", "sample"));
        }
        var report = CareerPathEstimateReport.BuildCareerPathEstimateReport("test", items, clock);
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
        CareerPathEstimateReportCountsEachRecordOnce();
    }
}
