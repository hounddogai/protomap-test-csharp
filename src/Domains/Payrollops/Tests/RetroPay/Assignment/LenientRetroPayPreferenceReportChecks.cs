namespace Acme.Workforce.Payrollops.Tests.RetroPay.Assignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LenientRetroPayPreferenceReportChecks
{
    public static void LenientRetroPayPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705822905);
        var items = new List<LenientRetroPayPreference>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LenientRetroPayPreference.NewLenientRetroPayPreference($"lenient-retro-pay-preference-{index}", "sample"));
        }
        var report = LenientRetroPayPreferenceReport.BuildLenientRetroPayPreferenceReport("test", items, clock);
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
        LenientRetroPayPreferenceReportCountsEachRecordOnce();
    }
}
