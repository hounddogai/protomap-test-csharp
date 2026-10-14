namespace Acme.Workforce.Contractors.Tests.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of manual offboarding segment code, run by the project's runner.</summary>
public static class StrictOffboardingOverrideReportChecks
{
    public static void StrictOffboardingOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707389803);
        var items = new List<StrictOffboardingOverride>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(StrictOffboardingOverride.NewStrictOffboardingOverride($"strict-offboarding-override-{index}", "sample"));
        }
        var report = StrictOffboardingOverrideReport.BuildStrictOffboardingOverrideReport("test", items, clock);
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
        StrictOffboardingOverrideReportCountsEachRecordOnce();
    }
}
