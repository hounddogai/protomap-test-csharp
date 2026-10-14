namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalAuditClaimReportChecks
{
    public static void SeasonalAuditClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700490461);
        var items = new List<SeasonalAuditClaim>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SeasonalAuditClaim.NewSeasonalAuditClaim($"seasonal-audit-claim-{index}", "sample"));
        }
        var report = SeasonalAuditClaimReport.BuildSeasonalAuditClaimReport("test", items, clock);
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
        SeasonalAuditClaimReportCountsEachRecordOnce();
    }
}
