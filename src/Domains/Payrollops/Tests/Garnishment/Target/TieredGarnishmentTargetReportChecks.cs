namespace Acme.Workforce.Payrollops.Tests.Garnishment.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class TieredGarnishmentTargetReportChecks
{
    public static void TieredGarnishmentTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709887931);
        var items = new List<TieredGarnishmentTarget>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TieredGarnishmentTarget.NewTieredGarnishmentTarget($"tiered-garnishment-target-{index}", "sample"));
        }
        var report = TieredGarnishmentTargetReport.BuildTieredGarnishmentTargetReport("test", items, clock);
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
        TieredGarnishmentTargetReportCountsEachRecordOnce();
    }
}
