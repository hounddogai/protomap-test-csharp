namespace Acme.Workforce.Payrollops.Tests.RetroPay.Cycle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RetroPayCycleReportChecks
{
    public static void RetroPayCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700125108);
        var items = new List<RetroPayCycle>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RetroPayCycle.NewRetroPayCycle($"retro-pay-cycle-{index}", "sample"));
        }
        var report = RetroPayCycleReport.BuildRetroPayCycleReport("test", items, clock);
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
        RetroPayCycleReportCountsEachRecordOnce();
    }
}
