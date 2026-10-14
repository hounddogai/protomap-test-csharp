namespace Acme.Workforce.Absence.Tests.ReturnPlan.Audit;

using Acme.Workforce.Absence.ReturnPlan.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class ReturnPlanDraftReportChecks
{
    public static void ReturnPlanDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703376332);
        var items = new List<ReturnPlanDraft>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ReturnPlanDraft.NewReturnPlanDraft($"return-plan-draft-{index}", "sample"));
        }
        var report = ReturnPlanDraftReport.BuildReturnPlanDraftReport("test", items, clock);
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
        ReturnPlanDraftReportCountsEachRecordOnce();
    }
}
