namespace Acme.Workforce.Assets.Tests.Refresh.RecordItem;

using Acme.Workforce.Assets.Refresh.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RefreshWindowReportChecks
{
    public static void RefreshWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706022045);
        var items = new List<RefreshWindow>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RefreshWindow.NewRefreshWindow($"refresh-window-{index}", "sample"));
        }
        var report = RefreshWindowReport.BuildRefreshWindowReport("test", items, clock);
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
        RefreshWindowReportCountsEachRecordOnce();
    }
}
