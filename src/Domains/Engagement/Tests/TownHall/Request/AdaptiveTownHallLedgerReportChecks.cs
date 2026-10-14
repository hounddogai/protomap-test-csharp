namespace Acme.Workforce.Engagement.Tests.TownHall.Request;

using Acme.Workforce.Engagement.TownHall.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdaptiveTownHallLedgerReportChecks
{
    public static void AdaptiveTownHallLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708831082);
        var items = new List<AdaptiveTownHallLedger>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdaptiveTownHallLedger.NewAdaptiveTownHallLedger($"adaptive-town-hall-ledger-{index}", "sample"));
        }
        var report = AdaptiveTownHallLedgerReport.BuildAdaptiveTownHallLedgerReport("test", items, clock);
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
        AdaptiveTownHallLedgerReportCountsEachRecordOnce();
    }
}
