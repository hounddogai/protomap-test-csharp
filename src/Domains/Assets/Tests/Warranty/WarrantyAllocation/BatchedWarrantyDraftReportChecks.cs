namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAllocation;

using Acme.Workforce.Assets.Warranty.WarrantyAllocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of warranty allocation code, run by the project's runner.</summary>
public static class BatchedWarrantyDraftReportChecks
{
    public static void BatchedWarrantyDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703708717);
        var items = new List<BatchedWarrantyDraft>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BatchedWarrantyDraft.NewBatchedWarrantyDraft($"batched-warranty-draft-{index}", "sample"));
        }
        var report = BatchedWarrantyDraftReport.BuildBatchedWarrantyDraftReport("test", items, clock);
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
        BatchedWarrantyDraftReportCountsEachRecordOnce();
    }
}
