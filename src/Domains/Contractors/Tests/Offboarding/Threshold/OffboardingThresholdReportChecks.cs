namespace Acme.Workforce.Contractors.Tests.Offboarding.Threshold;

using Acme.Workforce.Contractors.Offboarding.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class OffboardingThresholdReportChecks
{
    public static void OffboardingThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702643813);
        var items = new List<OffboardingThreshold>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(OffboardingThreshold.NewOffboardingThreshold($"offboarding-threshold-{index}", "sample"));
        }
        var report = OffboardingThresholdReport.BuildOffboardingThresholdReport("test", items, clock);
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
        OffboardingThresholdReportCountsEachRecordOnce();
    }
}
