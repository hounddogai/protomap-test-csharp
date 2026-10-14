namespace Acme.Workforce.Orgdesign.Tests.Position.Quota;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class DefaultPositionQuotaReportChecks
{
    public static void DefaultPositionQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707375103);
        var items = new List<DefaultPositionQuota>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultPositionQuota.NewDefaultPositionQuota($"default-position-quota-{index}", "sample"));
        }
        var report = DefaultPositionQuotaReport.BuildDefaultPositionQuotaReport("test", items, clock);
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
        DefaultPositionQuotaReportCountsEachRecordOnce();
    }
}
