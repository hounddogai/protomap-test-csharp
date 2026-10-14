namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedActionPlanDraftReportChecks
{
    public static void CappedActionPlanDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706079201);
        var items = new List<CappedActionPlanDraft>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedActionPlanDraft.NewCappedActionPlanDraft($"capped-action-plan-draft-{index}", "sample"));
        }
        var report = CappedActionPlanDraftReport.BuildCappedActionPlanDraftReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        CappedActionPlanDraftReportCountsEachRecordOnce();
    }
}
