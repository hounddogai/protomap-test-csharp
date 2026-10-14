namespace Acme.Workforce.Talent.Tests.Nomination.Profile;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class WeightedNominationVarianceReportChecks
{
    public static void WeightedNominationVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707704848);
        var items = new List<WeightedNominationVariance>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(WeightedNominationVariance.NewWeightedNominationVariance($"weighted-nomination-variance-{index}", "sample"));
        }
        var report = WeightedNominationVarianceReport.BuildWeightedNominationVarianceReport("test", items, clock);
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
        WeightedNominationVarianceReportCountsEachRecordOnce();
    }
}
