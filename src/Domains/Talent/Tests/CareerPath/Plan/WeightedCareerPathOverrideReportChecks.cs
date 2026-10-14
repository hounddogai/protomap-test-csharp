namespace Acme.Workforce.Talent.Tests.CareerPath.Plan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class WeightedCareerPathOverrideReportChecks
{
    public static void WeightedCareerPathOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704857696);
        var items = new List<WeightedCareerPathOverride>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(WeightedCareerPathOverride.NewWeightedCareerPathOverride($"weighted-career-path-override-{index}", "sample"));
        }
        var report = WeightedCareerPathOverrideReport.BuildWeightedCareerPathOverrideReport("test", items, clock);
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
        WeightedCareerPathOverrideReportCountsEachRecordOnce();
    }
}
