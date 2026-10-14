namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultActionPlanDraftReportChecks
{
    public static void DefaultActionPlanDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703181595);
        var items = new List<DefaultActionPlanDraft>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DefaultActionPlanDraft.NewDefaultActionPlanDraft($"default-action-plan-draft-{index}", "sample"));
        }
        var report = DefaultActionPlanDraftReport.BuildDefaultActionPlanDraftReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        DefaultActionPlanDraftReportCountsEachRecordOnce();
    }
}
