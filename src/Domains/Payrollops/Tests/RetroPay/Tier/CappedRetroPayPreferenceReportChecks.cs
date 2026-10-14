namespace Acme.Workforce.Payrollops.Tests.RetroPay.Tier;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class CappedRetroPayPreferenceReportChecks
{
    public static void CappedRetroPayPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708599244);
        var items = new List<CappedRetroPayPreference>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CappedRetroPayPreference.NewCappedRetroPayPreference($"capped-retro-pay-preference-{index}", "sample"));
        }
        var report = CappedRetroPayPreferenceReport.BuildCappedRetroPayPreferenceReport("test", items, clock);
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
        CappedRetroPayPreferenceReportCountsEachRecordOnce();
    }
}
