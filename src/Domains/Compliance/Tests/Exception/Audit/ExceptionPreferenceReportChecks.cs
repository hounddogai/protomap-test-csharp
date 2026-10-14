namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class ExceptionPreferenceReportChecks
{
    public static void ExceptionPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703591644);
        var items = new List<ExceptionPreference>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ExceptionPreference.NewExceptionPreference($"exception-preference-{index}", "sample"));
        }
        var report = ExceptionPreferenceReport.BuildExceptionPreferenceReport("test", items, clock);
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
        ExceptionPreferenceReportCountsEachRecordOnce();
    }
}
