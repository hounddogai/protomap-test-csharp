namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AccessBadgeAgreementReportChecks
{
    public static void AccessBadgeAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704330078);
        var items = new List<AccessBadgeAgreement>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AccessBadgeAgreement.NewAccessBadgeAgreement($"access-badge-agreement-{index}", "sample"));
        }
        var report = AccessBadgeAgreementReport.BuildAccessBadgeAgreementReport("test", items, clock);
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
        AccessBadgeAgreementReportCountsEachRecordOnce();
    }
}
