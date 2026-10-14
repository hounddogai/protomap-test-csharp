namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class PendingCareerPathBatchReportChecks
{
    public static void PendingCareerPathBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709907248);
        var items = new List<PendingCareerPathBatch>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PendingCareerPathBatch.NewPendingCareerPathBatch($"pending-career-path-batch-{index}", "sample"));
        }
        var report = PendingCareerPathBatchReport.BuildPendingCareerPathBatchReport("test", items, clock);
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
        PendingCareerPathBatchReportCountsEachRecordOnce();
    }
}
