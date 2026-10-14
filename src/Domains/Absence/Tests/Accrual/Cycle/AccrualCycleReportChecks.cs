namespace Acme.Workforce.Absence.Tests.Accrual.Cycle;

using Acme.Workforce.Absence.Accrual.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AccrualCycleReportChecks
{
    public static void AccrualCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709146279);
        var items = new List<AccrualCycle>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AccrualCycle.NewAccrualCycle($"accrual-cycle-{index}", "sample"));
        }
        var report = AccrualCycleReport.BuildAccrualCycleReport("test", items, clock);
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
        AccrualCycleReportCountsEachRecordOnce();
    }
}
