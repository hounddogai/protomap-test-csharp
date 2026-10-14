namespace Acme.Workforce.Payrollops.Tests.RetroPay.Batch;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RetroPayOverrideReportChecks
{
    public static void RetroPayOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709737561);
        var items = new List<RetroPayOverride>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RetroPayOverride.NewRetroPayOverride($"retro-pay-override-{index}", "sample"));
        }
        var report = RetroPayOverrideReport.BuildRetroPayOverrideReport("test", items, clock);
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
        RetroPayOverrideReportCountsEachRecordOnce();
    }
}
