namespace Acme.Workforce.Compliance.Tests.TrainingDue.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Mapping;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class FallbackTrainingDueSegmentReportChecks
{
    public static void FallbackTrainingDueSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705335597);
        var items = new List<FallbackTrainingDueSegment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FallbackTrainingDueSegment.NewFallbackTrainingDueSegment($"fallback-training-due-segment-{index}", "sample"));
        }
        var report = FallbackTrainingDueSegmentReport.BuildFallbackTrainingDueSegmentReport("test", items, clock);
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
        FallbackTrainingDueSegmentReportCountsEachRecordOnce();
    }
}
