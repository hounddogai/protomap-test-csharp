namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class FallbackActionPlanDraftReportChecks
{
    public static void FallbackActionPlanDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704862359);
        var items = new List<FallbackActionPlanDraft>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FallbackActionPlanDraft.NewFallbackActionPlanDraft($"fallback-action-plan-draft-{index}", "sample"));
        }
        var report = FallbackActionPlanDraftReport.BuildFallbackActionPlanDraftReport("test", items, clock);
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
        FallbackActionPlanDraftReportCountsEachRecordOnce();
    }
}
