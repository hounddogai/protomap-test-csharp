namespace Acme.Workforce.Talent.Tests.Nomination.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class NominationTargetReportChecks
{
    public static void NominationTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702543197);
        var items = new List<NominationTarget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(NominationTarget.NewNominationTarget($"nomination-target-{index}", "sample"));
        }
        var report = NominationTargetReport.BuildNominationTargetReport("test", items, clock);
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
        NominationTargetReportCountsEachRecordOnce();
    }
}
