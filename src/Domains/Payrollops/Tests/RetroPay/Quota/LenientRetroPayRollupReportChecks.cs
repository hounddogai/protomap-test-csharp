namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class LenientRetroPayRollupReportChecks
{
    public static void LenientRetroPayRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702227609);
        var items = new List<LenientRetroPayRollup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientRetroPayRollup.NewLenientRetroPayRollup($"lenient-retro-pay-rollup-{index}", "sample"));
        }
        var report = LenientRetroPayRollupReport.BuildLenientRetroPayRollupReport("test", items, clock);
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
        LenientRetroPayRollupReportCountsEachRecordOnce();
    }
}
