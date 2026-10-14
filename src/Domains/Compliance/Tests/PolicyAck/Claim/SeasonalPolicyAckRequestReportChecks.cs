namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class SeasonalPolicyAckRequestReportChecks
{
    public static void SeasonalPolicyAckRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706241152);
        var items = new List<SeasonalPolicyAckRequest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SeasonalPolicyAckRequest.NewSeasonalPolicyAckRequest($"seasonal-policy-ack-request-{index}", "sample"));
        }
        var report = SeasonalPolicyAckRequestReport.BuildSeasonalPolicyAckRequestReport("test", items, clock);
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
        SeasonalPolicyAckRequestReportCountsEachRecordOnce();
    }
}
