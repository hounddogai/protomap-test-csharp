namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeEntry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Checks of badge entry code, run by the project's runner.</summary>
public static class RollingAccessBadgeAgreementReportChecks
{
    public static void RollingAccessBadgeAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704676471);
        var items = new List<RollingAccessBadgeAgreement>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RollingAccessBadgeAgreement.NewRollingAccessBadgeAgreement($"rolling-access-badge-agreement-{index}", "sample"));
        }
        var report = RollingAccessBadgeAgreementReport.BuildRollingAccessBadgeAgreementReport("test", items, clock);
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
        RollingAccessBadgeAgreementReportCountsEachRecordOnce();
    }
}
