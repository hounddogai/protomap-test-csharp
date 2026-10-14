namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PrimaryGarnishmentMappingReportChecks
{
    public static void PrimaryGarnishmentMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709302579);
        var items = new List<PrimaryGarnishmentMapping>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PrimaryGarnishmentMapping.NewPrimaryGarnishmentMapping($"primary-garnishment-mapping-{index}", "sample"));
        }
        var report = PrimaryGarnishmentMappingReport.BuildPrimaryGarnishmentMappingReport("test", items, clock);
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
        PrimaryGarnishmentMappingReportCountsEachRecordOnce();
    }
}
