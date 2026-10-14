namespace Acme.Workforce.Compensation.Tests.PayBand.Preference;

using Acme.Workforce.Compensation.PayBand.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PayBandPreferenceReportChecks
{
    public static void PayBandPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700028036);
        var items = new List<PayBandPreference>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PayBandPreference.NewPayBandPreference($"pay-band-preference-{index}", "sample"));
        }
        var report = PayBandPreferenceReport.BuildPayBandPreferenceReport("test", items, clock);
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
        PayBandPreferenceReportCountsEachRecordOnce();
    }
}
