namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class LegacyHeadcountQuotaReportChecks
{
    public static void LegacyHeadcountQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708104865);
        var items = new List<LegacyHeadcountQuota>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LegacyHeadcountQuota.NewLegacyHeadcountQuota($"legacy-headcount-quota-{index}", "sample"));
        }
        var report = LegacyHeadcountQuotaReport.BuildLegacyHeadcountQuotaReport("test", items, clock);
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
        LegacyHeadcountQuotaReportCountsEachRecordOnce();
    }
}
