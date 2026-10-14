namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayPlanReportChecks
{
    public static void RetroPayPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701876898);
        var items = new List<RetroPayPlan>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RetroPayPlan.NewRetroPayPlan($"retro-pay-plan-{index}", "sample"));
        }
        var report = RetroPayPlanReport.BuildRetroPayPlanReport("test", items, clock);
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
        RetroPayPlanReportCountsEachRecordOnce();
    }
}
