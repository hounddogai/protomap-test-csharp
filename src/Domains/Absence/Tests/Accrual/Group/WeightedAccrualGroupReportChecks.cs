namespace Acme.Workforce.Absence.Tests.Accrual.Group;

using Acme.Workforce.Absence.Accrual.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class WeightedAccrualGroupReportChecks
{
    public static void WeightedAccrualGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701581047);
        var items = new List<WeightedAccrualGroup>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WeightedAccrualGroup.NewWeightedAccrualGroup($"weighted-accrual-group-{index}", "sample"));
        }
        var report = WeightedAccrualGroupReport.BuildWeightedAccrualGroupReport("test", items, clock);
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
        WeightedAccrualGroupReportCountsEachRecordOnce();
    }
}
