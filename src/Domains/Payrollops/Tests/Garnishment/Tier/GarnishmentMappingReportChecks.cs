namespace Acme.Workforce.Payrollops.Tests.Garnishment.Tier;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class GarnishmentMappingReportChecks
{
    public static void GarnishmentMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703340329);
        var items = new List<GarnishmentMapping>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(GarnishmentMapping.NewGarnishmentMapping($"garnishment-mapping-{index}", "sample"));
        }
        var report = GarnishmentMappingReport.BuildGarnishmentMappingReport("test", items, clock);
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
        GarnishmentMappingReportCountsEachRecordOnce();
    }
}
