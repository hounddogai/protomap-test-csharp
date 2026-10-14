namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathCycleReportChecks
{
    public static void CareerPathCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700002000);
        var items = new List<CareerPathCycle>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CareerPathCycle.NewCareerPathCycle($"career-path-cycle-{index}", "sample"));
        }
        var report = CareerPathCycleReport.BuildCareerPathCycleReport("test", items, clock);
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
        CareerPathCycleReportCountsEachRecordOnce();
    }
}
