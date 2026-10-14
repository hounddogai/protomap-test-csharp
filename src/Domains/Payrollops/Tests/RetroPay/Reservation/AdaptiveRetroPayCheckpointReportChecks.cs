namespace Acme.Workforce.Payrollops.Tests.RetroPay.Reservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AdaptiveRetroPayCheckpointReportChecks
{
    public static void AdaptiveRetroPayCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706305524);
        var items = new List<AdaptiveRetroPayCheckpoint>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdaptiveRetroPayCheckpoint.NewAdaptiveRetroPayCheckpoint($"adaptive-retro-pay-checkpoint-{index}", "sample"));
        }
        var report = AdaptiveRetroPayCheckpointReport.BuildAdaptiveRetroPayCheckpointReport("test", items, clock);
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
        AdaptiveRetroPayCheckpointReportCountsEachRecordOnce();
    }
}
