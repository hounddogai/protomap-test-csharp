namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class CappedCostModelDraftReportChecks
{
    public static void CappedCostModelDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708252730);
        var items = new List<CappedCostModelDraft>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedCostModelDraft.NewCappedCostModelDraft($"capped-cost-model-draft-{index}", "sample"));
        }
        var report = CappedCostModelDraftReport.BuildCappedCostModelDraftReport("test", items, clock);
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
        CappedCostModelDraftReportCountsEachRecordOnce();
    }
}
