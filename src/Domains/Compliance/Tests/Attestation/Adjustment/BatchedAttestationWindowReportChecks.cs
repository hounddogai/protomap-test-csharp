namespace Acme.Workforce.Compliance.Tests.Attestation.Adjustment;

using Acme.Workforce.Compliance.Attestation.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class BatchedAttestationWindowReportChecks
{
    public static void BatchedAttestationWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708030071);
        var items = new List<BatchedAttestationWindow>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BatchedAttestationWindow.NewBatchedAttestationWindow($"batched-attestation-window-{index}", "sample"));
        }
        var report = BatchedAttestationWindowReport.BuildBatchedAttestationWindowReport("test", items, clock);
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
        BatchedAttestationWindowReportCountsEachRecordOnce();
    }
}
