namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

/// <summary>Checks of garnishment allocation code, run by the project's runner.</summary>
public static class GarnishmentBundleReportChecks
{
    public static void GarnishmentBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705128313);
        var items = new List<GarnishmentBundle>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(GarnishmentBundle.NewGarnishmentBundle($"garnishment-bundle-{index}", "sample"));
        }
        var report = GarnishmentBundleReport.BuildGarnishmentBundleReport("test", items, clock);
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
        GarnishmentBundleReportCountsEachRecordOnce();
    }
}
