namespace Acme.Workforce.Talent.Tests.Nomination.NominationWindow;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>Checks of nomination window code, run by the project's runner.</summary>
public static class NominationSlotReportChecks
{
    public static void NominationSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707930990);
        var items = new List<NominationSlot>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(NominationSlot.NewNominationSlot($"nomination-slot-{index}", "sample"));
        }
        var report = NominationSlotReport.BuildNominationSlotReport("test", items, clock);
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
        NominationSlotReportCountsEachRecordOnce();
    }
}
