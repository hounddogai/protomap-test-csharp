namespace Acme.Workforce.Talent.Tests.Nomination.Window;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class NominationWindowReportChecks
{
    public static void NominationWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708812313);
        var items = new List<NominationWindow>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(NominationWindow.NewNominationWindow($"nomination-window-{index}", "sample"));
        }
        var report = NominationWindowReport.BuildNominationWindowReport("test", items, clock);
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
        NominationWindowReportCountsEachRecordOnce();
    }
}
