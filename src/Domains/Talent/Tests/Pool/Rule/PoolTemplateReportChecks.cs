namespace Acme.Workforce.Talent.Tests.Pool.Rule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PoolTemplateReportChecks
{
    public static void PoolTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709383236);
        var items = new List<PoolTemplate>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PoolTemplate.NewPoolTemplate($"pool-template-{index}", "sample"));
        }
        var report = PoolTemplateReport.BuildPoolTemplateReport("test", items, clock);
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
        PoolTemplateReportCountsEachRecordOnce();
    }
}
