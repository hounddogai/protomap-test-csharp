namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class WeightedAdjustmentDraftReportChecks
{
    public static void WeightedAdjustmentDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705321807);
        var items = new List<WeightedAdjustmentDraft>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(WeightedAdjustmentDraft.NewWeightedAdjustmentDraft($"weighted-adjustment-draft-{index}", "sample"));
        }
        var report = WeightedAdjustmentDraftReport.BuildWeightedAdjustmentDraftReport("test", items, clock);
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
        WeightedAdjustmentDraftReportCountsEachRecordOnce();
    }
}
