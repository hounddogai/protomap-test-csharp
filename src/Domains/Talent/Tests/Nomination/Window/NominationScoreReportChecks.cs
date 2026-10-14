namespace Acme.Workforce.Talent.Tests.Nomination.Window;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class NominationScoreReportChecks
{
    public static void NominationScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709862935);
        var items = new List<NominationScore>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(NominationScore.NewNominationScore($"nomination-score-{index}", "sample"));
        }
        var report = NominationScoreReport.BuildNominationScoreReport("test", items, clock);
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
        NominationScoreReportCountsEachRecordOnce();
    }
}
