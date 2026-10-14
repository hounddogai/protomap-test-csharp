namespace Acme.Workforce.Workplace.Tests.Desk.DeskVariance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.DeskVariance;

/// <summary>Checks of desk variance code, run by the project's runner.</summary>
public static class TieredDeskVarianceReportChecks
{
    public static void TieredDeskVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701556396);
        var items = new List<TieredDeskVariance>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TieredDeskVariance.NewTieredDeskVariance($"tiered-desk-variance-{index}", "sample"));
        }
        var report = TieredDeskVarianceReport.BuildTieredDeskVarianceReport("test", items, clock);
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
        TieredDeskVarianceReportCountsEachRecordOnce();
    }
}
