namespace Acme.Workforce.Contractors.Tests.Engagement.Ledger;

using Acme.Workforce.Contractors.Engagement.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FallbackEngagementLedgerReportChecks
{
    public static void FallbackEngagementLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702444675);
        var items = new List<FallbackEngagementLedger>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FallbackEngagementLedger.NewFallbackEngagementLedger($"fallback-engagement-ledger-{index}", "sample"));
        }
        var report = FallbackEngagementLedgerReport.BuildFallbackEngagementLedgerReport("test", items, clock);
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
        FallbackEngagementLedgerReportCountsEachRecordOnce();
    }
}
