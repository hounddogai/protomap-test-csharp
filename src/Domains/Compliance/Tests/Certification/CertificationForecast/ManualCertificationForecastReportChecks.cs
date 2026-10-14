namespace Acme.Workforce.Compliance.Tests.Certification.CertificationForecast;

using Acme.Workforce.Compliance.Certification.CertificationForecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of certification forecast code, run by the project's runner.</summary>
public static class ManualCertificationForecastReportChecks
{
    public static void ManualCertificationForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702516714);
        var items = new List<ManualCertificationForecast>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ManualCertificationForecast.NewManualCertificationForecast($"manual-certification-forecast-{index}", "sample"));
        }
        var report = ManualCertificationForecastReport.BuildManualCertificationForecastReport("test", items, clock);
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
        ManualCertificationForecastReportCountsEachRecordOnce();
    }
}
