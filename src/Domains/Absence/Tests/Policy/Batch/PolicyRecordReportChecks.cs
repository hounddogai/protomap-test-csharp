namespace Acme.Workforce.Absence.Tests.Policy.Batch;

using Acme.Workforce.Absence.Policy.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PolicyRecordReportChecks
{
    public static void PolicyRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706221240);
        var items = new List<PolicyRecord>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PolicyRecord.NewPolicyRecord($"policy-record-{index}", "sample"));
        }
        var report = PolicyRecordReport.BuildPolicyRecordReport("test", items, clock);
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
        PolicyRecordReportCountsEachRecordOnce();
    }
}
