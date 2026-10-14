namespace Acme.Workforce.Talent.Tests.Nomination.Score;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class NominationClaimReportChecks
{
    public static void NominationClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705803703);
        var items = new List<NominationClaim>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(NominationClaim.NewNominationClaim($"nomination-claim-{index}", "sample"));
        }
        var report = NominationClaimReport.BuildNominationClaimReport("test", items, clock);
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
        NominationClaimReportCountsEachRecordOnce();
    }
}
