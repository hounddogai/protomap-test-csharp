namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathRollupReportChecks
{
    public static void CareerPathRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704810306);
        var items = new List<CareerPathRollup>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CareerPathRollup.NewCareerPathRollup($"career-path-rollup-{index}", "sample"));
        }
        var report = CareerPathRollupReport.BuildCareerPathRollupReport("test", items, clock);
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
        CareerPathRollupReportCountsEachRecordOnce();
    }
}
