namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationProfileReportChecks
{
    public static void NominationProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701441475);
        var items = new List<NominationProfile>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(NominationProfile.NewNominationProfile($"nomination-profile-{index}", "sample"));
        }
        var report = NominationProfileReport.BuildNominationProfileReport("test", items, clock);
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
        NominationProfileReportCountsEachRecordOnce();
    }
}
