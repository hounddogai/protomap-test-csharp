namespace Acme.Workforce.Talent.Tests.CareerPath.Allocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class CareerPathAdjustmentReportChecks
{
    public static void CareerPathAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708545914);
        var items = new List<CareerPathAdjustment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CareerPathAdjustment.NewCareerPathAdjustment($"career-path-adjustment-{index}", "sample"));
        }
        var report = CareerPathAdjustmentReport.BuildCareerPathAdjustmentReport("test", items, clock);
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
        CareerPathAdjustmentReportCountsEachRecordOnce();
    }
}
