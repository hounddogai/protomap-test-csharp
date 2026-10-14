namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class RetroPayVarianceReportChecks
{
    public static void RetroPayVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705535104);
        var items = new List<RetroPayVariance>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RetroPayVariance.NewRetroPayVariance($"retro-pay-variance-{index}", "sample"));
        }
        var report = RetroPayVarianceReport.BuildRetroPayVarianceReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RetroPayVarianceReportCountsEachRecordOnce();
    }
}
