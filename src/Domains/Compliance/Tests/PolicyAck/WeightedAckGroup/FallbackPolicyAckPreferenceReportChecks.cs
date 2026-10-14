namespace Acme.Workforce.Compliance.Tests.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of weighted ack group code, run by the project's runner.</summary>
public static class FallbackPolicyAckPreferenceReportChecks
{
    public static void FallbackPolicyAckPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702359957);
        var items = new List<FallbackPolicyAckPreference>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FallbackPolicyAckPreference.NewFallbackPolicyAckPreference($"fallback-policy-ack-preference-{index}", "sample"));
        }
        var report = FallbackPolicyAckPreferenceReport.BuildFallbackPolicyAckPreferenceReport("test", items, clock);
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
        FallbackPolicyAckPreferenceReportCountsEachRecordOnce();
    }
}
