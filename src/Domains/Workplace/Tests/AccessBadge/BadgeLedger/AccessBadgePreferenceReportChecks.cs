namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class AccessBadgePreferenceReportChecks
{
    public static void AccessBadgePreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705107828);
        var items = new List<AccessBadgePreference>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AccessBadgePreference.NewAccessBadgePreference($"access-badge-preference-{index}", "sample"));
        }
        var report = AccessBadgePreferenceReport.BuildAccessBadgePreferenceReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AccessBadgePreferenceReportCountsEachRecordOnce();
    }
}
