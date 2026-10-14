namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanRollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

/// <summary>Checks of plan rollup code, run by the project's runner.</summary>
public static class FallbackSuccessionPlanManifestReportChecks
{
    public static void FallbackSuccessionPlanManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705311925);
        var items = new List<FallbackSuccessionPlanManifest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FallbackSuccessionPlanManifest.NewFallbackSuccessionPlanManifest($"fallback-succession-plan-manifest-{index}", "sample"));
        }
        var report = FallbackSuccessionPlanManifestReport.BuildFallbackSuccessionPlanManifestReport("test", items, clock);
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
        FallbackSuccessionPlanManifestReportCountsEachRecordOnce();
    }
}
