namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class FallbackSuccessionPlanEntryReportChecks
{
    public static void FallbackSuccessionPlanEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706995447);
        var items = new List<FallbackSuccessionPlanEntry>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FallbackSuccessionPlanEntry.NewFallbackSuccessionPlanEntry($"fallback-succession-plan-entry-{index}", "sample"));
        }
        var report = FallbackSuccessionPlanEntryReport.BuildFallbackSuccessionPlanEntryReport("test", items, clock);
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
        FallbackSuccessionPlanEntryReportCountsEachRecordOnce();
    }
}
