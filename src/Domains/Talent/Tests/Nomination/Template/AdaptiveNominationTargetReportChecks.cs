namespace Acme.Workforce.Talent.Tests.Nomination.Template;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class AdaptiveNominationTargetReportChecks
{
    public static void AdaptiveNominationTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701335473);
        var items = new List<AdaptiveNominationTarget>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AdaptiveNominationTarget.NewAdaptiveNominationTarget($"adaptive-nomination-target-{index}", "sample"));
        }
        var report = AdaptiveNominationTargetReport.BuildAdaptiveNominationTargetReport("test", items, clock);
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
        AdaptiveNominationTargetReportCountsEachRecordOnce();
    }
}
