namespace Acme.Workforce.Talent.Tests.Nomination.NominationWindow;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>Checks of nomination window code, run by the project's runner.</summary>
public static class CappedNominationWindowReportChecks
{
    public static void CappedNominationWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701874907);
        var items = new List<CappedNominationWindow>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedNominationWindow.NewCappedNominationWindow($"capped-nomination-window-{index}", "sample"));
        }
        var report = CappedNominationWindowReport.BuildCappedNominationWindowReport("test", items, clock);
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
        CappedNominationWindowReportCountsEachRecordOnce();
    }
}
