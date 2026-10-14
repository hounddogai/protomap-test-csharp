namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DefaultActionPlanManifestReportChecks
{
    public static void DefaultActionPlanManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703758878);
        var items = new List<DefaultActionPlanManifest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(DefaultActionPlanManifest.NewDefaultActionPlanManifest($"default-action-plan-manifest-{index}", "sample"));
        }
        var report = DefaultActionPlanManifestReport.BuildDefaultActionPlanManifestReport("test", items, clock);
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
        DefaultActionPlanManifestReportCountsEachRecordOnce();
    }
}
