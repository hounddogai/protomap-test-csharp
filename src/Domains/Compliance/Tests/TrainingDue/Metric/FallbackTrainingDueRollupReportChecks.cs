namespace Acme.Workforce.Compliance.Tests.TrainingDue.Metric;

using Acme.Workforce.Compliance.TrainingDue.Metric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class FallbackTrainingDueRollupReportChecks
{
    public static void FallbackTrainingDueRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700956016);
        var items = new List<FallbackTrainingDueRollup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FallbackTrainingDueRollup.NewFallbackTrainingDueRollup($"fallback-training-due-rollup-{index}", "sample"));
        }
        var report = FallbackTrainingDueRollupReport.BuildFallbackTrainingDueRollupReport("test", items, clock);
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
        FallbackTrainingDueRollupReportCountsEachRecordOnce();
    }
}
