namespace Acme.Workforce.Contractors.Tests.Engagement.Ledger;

using Acme.Workforce.Contractors.Engagement.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CappedEngagementEntryReportChecks
{
    public static void CappedEngagementEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706580144);
        var items = new List<CappedEngagementEntry>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedEngagementEntry.NewCappedEngagementEntry($"capped-engagement-entry-{index}", "sample"));
        }
        var report = CappedEngagementEntryReport.BuildCappedEngagementEntryReport("test", items, clock);
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
        CappedEngagementEntryReportCountsEachRecordOnce();
    }
}
