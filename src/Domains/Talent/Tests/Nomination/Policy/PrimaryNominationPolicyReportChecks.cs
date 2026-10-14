namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PrimaryNominationPolicyReportChecks
{
    public static void PrimaryNominationPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709929531);
        var items = new List<PrimaryNominationPolicy>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PrimaryNominationPolicy.NewPrimaryNominationPolicy($"primary-nomination-policy-{index}", "sample"));
        }
        var report = PrimaryNominationPolicyReport.BuildPrimaryNominationPolicyReport("test", items, clock);
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
        PrimaryNominationPolicyReportCountsEachRecordOnce();
    }
}
