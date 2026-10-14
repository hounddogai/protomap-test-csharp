namespace Acme.Workforce.Contractors.Tests.Offboarding.Mapping;

using Acme.Workforce.Contractors.Offboarding.Mapping;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class WeightedOffboardingThresholdReportChecks
{
    public static void WeightedOffboardingThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705326922);
        var items = new List<WeightedOffboardingThreshold>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(WeightedOffboardingThreshold.NewWeightedOffboardingThreshold($"weighted-offboarding-threshold-{index}", "sample"));
        }
        var report = WeightedOffboardingThresholdReport.BuildWeightedOffboardingThresholdReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        WeightedOffboardingThresholdReportCountsEachRecordOnce();
    }
}
