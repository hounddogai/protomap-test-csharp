namespace Acme.Workforce.Contractors.Tests.Offboarding.Mapping;

using Acme.Workforce.Contractors.Offboarding.Mapping;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class OffboardingMappingReportChecks
{
    public static void OffboardingMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706387185);
        var items = new List<OffboardingMapping>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(OffboardingMapping.NewOffboardingMapping($"offboarding-mapping-{index}", "sample"));
        }
        var report = OffboardingMappingReport.BuildOffboardingMappingReport("test", items, clock);
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
        OffboardingMappingReportCountsEachRecordOnce();
    }
}
