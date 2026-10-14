namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PendingSpanClaimReportChecks
{
    public static void PendingSpanClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706877665);
        var items = new List<PendingSpanClaim>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PendingSpanClaim.NewPendingSpanClaim($"pending-span-claim-{index}", "sample"));
        }
        var report = PendingSpanClaimReport.BuildPendingSpanClaimReport("test", items, clock);
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
        PendingSpanClaimReportCountsEachRecordOnce();
    }
}
