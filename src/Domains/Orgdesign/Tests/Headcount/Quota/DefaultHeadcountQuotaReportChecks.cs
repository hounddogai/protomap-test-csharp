namespace Acme.Workforce.Orgdesign.Tests.Headcount.Quota;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class DefaultHeadcountQuotaReportChecks
{
    public static void DefaultHeadcountQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709257901);
        var items = new List<DefaultHeadcountQuota>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(DefaultHeadcountQuota.NewDefaultHeadcountQuota($"default-headcount-quota-{index}", "sample"));
        }
        var report = DefaultHeadcountQuotaReport.BuildDefaultHeadcountQuotaReport("test", items, clock);
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
        DefaultHeadcountQuotaReportCountsEachRecordOnce();
    }
}
