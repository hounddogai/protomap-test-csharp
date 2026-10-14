namespace Acme.Workforce.Talent.Tests.CareerPath.Exception;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BatchedCareerPathPlanReportChecks
{
    public static void BatchedCareerPathPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705058017);
        var items = new List<BatchedCareerPathPlan>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BatchedCareerPathPlan.NewBatchedCareerPathPlan($"batched-career-path-plan-{index}", "sample"));
        }
        var report = BatchedCareerPathPlanReport.BuildBatchedCareerPathPlanReport("test", items, clock);
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
        BatchedCareerPathPlanReportCountsEachRecordOnce();
    }
}
