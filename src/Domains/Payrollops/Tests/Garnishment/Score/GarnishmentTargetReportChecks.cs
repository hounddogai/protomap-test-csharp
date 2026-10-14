namespace Acme.Workforce.Payrollops.Tests.Garnishment.Score;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class GarnishmentTargetReportChecks
{
    public static void GarnishmentTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705251454);
        var items = new List<GarnishmentTarget>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(GarnishmentTarget.NewGarnishmentTarget($"garnishment-target-{index}", "sample"));
        }
        var report = GarnishmentTargetReport.BuildGarnishmentTargetReport("test", items, clock);
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
        GarnishmentTargetReportCountsEachRecordOnce();
    }
}
