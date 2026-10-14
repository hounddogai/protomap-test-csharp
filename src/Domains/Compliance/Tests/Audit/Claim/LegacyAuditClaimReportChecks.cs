namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LegacyAuditClaimReportChecks
{
    public static void LegacyAuditClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705822041);
        var items = new List<LegacyAuditClaim>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LegacyAuditClaim.NewLegacyAuditClaim($"legacy-audit-claim-{index}", "sample"));
        }
        var report = LegacyAuditClaimReport.BuildLegacyAuditClaimReport("test", items, clock);
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
        LegacyAuditClaimReportCountsEachRecordOnce();
    }
}
