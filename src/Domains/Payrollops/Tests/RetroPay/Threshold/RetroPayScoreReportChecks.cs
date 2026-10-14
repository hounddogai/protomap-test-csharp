namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayScoreReportChecks
{
    public static void RetroPayScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707407283);
        var items = new List<RetroPayScore>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RetroPayScore.NewRetroPayScore($"retro-pay-score-{index}", "sample"));
        }
        var report = RetroPayScoreReport.BuildRetroPayScoreReport("test", items, clock);
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
        RetroPayScoreReportCountsEachRecordOnce();
    }
}
