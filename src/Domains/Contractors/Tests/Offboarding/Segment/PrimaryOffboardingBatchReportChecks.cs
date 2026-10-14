namespace Acme.Workforce.Contractors.Tests.Offboarding.Segment;

using Acme.Workforce.Contractors.Offboarding.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PrimaryOffboardingBatchReportChecks
{
    public static void PrimaryOffboardingBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704603152);
        var items = new List<PrimaryOffboardingBatch>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PrimaryOffboardingBatch.NewPrimaryOffboardingBatch($"primary-offboarding-batch-{index}", "sample"));
        }
        var report = PrimaryOffboardingBatchReport.BuildPrimaryOffboardingBatchReport("test", items, clock);
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
        PrimaryOffboardingBatchReportCountsEachRecordOnce();
    }
}
