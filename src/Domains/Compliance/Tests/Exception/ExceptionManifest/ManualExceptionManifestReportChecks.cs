namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class ManualExceptionManifestReportChecks
{
    public static void ManualExceptionManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700131718);
        var items = new List<ManualExceptionManifest>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ManualExceptionManifest.NewManualExceptionManifest($"manual-exception-manifest-{index}", "sample"));
        }
        var report = ManualExceptionManifestReport.BuildManualExceptionManifestReport("test", items, clock);
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
        ManualExceptionManifestReportCountsEachRecordOnce();
    }
}
