namespace Acme.Workforce.Compliance.Tests.PolicyAck.Cycle;

using Acme.Workforce.Compliance.PolicyAck.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ManualPolicyAckGroupReportChecks
{
    public static void ManualPolicyAckGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702959475);
        var items = new List<ManualPolicyAckGroup>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ManualPolicyAckGroup.NewManualPolicyAckGroup($"manual-policy-ack-group-{index}", "sample"));
        }
        var report = ManualPolicyAckGroupReport.BuildManualPolicyAckGroupReport("test", items, clock);
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
        ManualPolicyAckGroupReportCountsEachRecordOnce();
    }
}
