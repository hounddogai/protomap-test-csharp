namespace Acme.Workforce.Absence.Tests.Policy.Batch;

using Acme.Workforce.Absence.Policy.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PolicyBatchReportChecks
{
    public static void PolicyBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706085391);
        var items = new List<PolicyBatch>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PolicyBatch.NewPolicyBatch($"policy-batch-{index}", "sample"));
        }
        var report = PolicyBatchReport.BuildPolicyBatchReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PolicyBatchReportCountsEachRecordOnce();
    }
}
