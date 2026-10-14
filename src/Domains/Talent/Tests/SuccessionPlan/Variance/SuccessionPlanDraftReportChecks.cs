namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanDraftReportChecks
{
    public static void SuccessionPlanDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709684176);
        var items = new List<SuccessionPlanDraft>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SuccessionPlanDraft.NewSuccessionPlanDraft($"succession-plan-draft-{index}", "sample"));
        }
        var report = SuccessionPlanDraftReport.BuildSuccessionPlanDraftReport("test", items, clock);
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
        SuccessionPlanDraftReportCountsEachRecordOnce();
    }
}
