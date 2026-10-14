namespace Acme.Workforce.Workplace.Tests.FloorPlan.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FloorPlanDraftReportChecks
{
    public static void FloorPlanDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707248837);
        var items = new List<FloorPlanDraft>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FloorPlanDraft.NewFloorPlanDraft($"floor-plan-draft-{index}", "sample"));
        }
        var report = FloorPlanDraftReport.BuildFloorPlanDraftReport("test", items, clock);
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
        FloorPlanDraftReportCountsEachRecordOnce();
    }
}
