namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class NominationPolicyReportChecks
{
    public static void NominationPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708339717);
        var items = new List<NominationPolicy>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(NominationPolicy.NewNominationPolicy($"nomination-policy-{index}", "sample"));
        }
        var report = NominationPolicyReport.BuildNominationPolicyReport("test", items, clock);
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
        NominationPolicyReportCountsEachRecordOnce();
    }
}
