namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class BatchedPolicyAckSnapshotReportChecks
{
    public static void BatchedPolicyAckSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700881617);
        var items = new List<BatchedPolicyAckSnapshot>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BatchedPolicyAckSnapshot.NewBatchedPolicyAckSnapshot($"batched-policy-ack-snapshot-{index}", "sample"));
        }
        var report = BatchedPolicyAckSnapshotReport.BuildBatchedPolicyAckSnapshotReport("test", items, clock);
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
        BatchedPolicyAckSnapshotReportCountsEachRecordOnce();
    }
}
