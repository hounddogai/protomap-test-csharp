namespace Acme.Workforce.Talent.Tests.Nomination.Cycle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveNominationVarianceReportChecks
{
    public static void AdaptiveNominationVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705652005);
        var items = new List<AdaptiveNominationVariance>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AdaptiveNominationVariance.NewAdaptiveNominationVariance($"adaptive-nomination-variance-{index}", "sample"));
        }
        var report = AdaptiveNominationVarianceReport.BuildAdaptiveNominationVarianceReport("test", items, clock);
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
        AdaptiveNominationVarianceReportCountsEachRecordOnce();
    }
}
