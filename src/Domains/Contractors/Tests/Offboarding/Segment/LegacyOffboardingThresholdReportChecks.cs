namespace Acme.Workforce.Contractors.Tests.Offboarding.Segment;

using Acme.Workforce.Contractors.Offboarding.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class LegacyOffboardingThresholdReportChecks
{
    public static void LegacyOffboardingThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706923119);
        var items = new List<LegacyOffboardingThreshold>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LegacyOffboardingThreshold.NewLegacyOffboardingThreshold($"legacy-offboarding-threshold-{index}", "sample"));
        }
        var report = LegacyOffboardingThresholdReport.BuildLegacyOffboardingThresholdReport("test", items, clock);
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
        LegacyOffboardingThresholdReportCountsEachRecordOnce();
    }
}
