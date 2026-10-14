namespace Acme.Workforce.Compliance.Tests.Exception.Manifest;

using Acme.Workforce.Compliance.Exception.Manifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ExceptionAgreementReportChecks
{
    public static void ExceptionAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703740447);
        var items = new List<ExceptionAgreement>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ExceptionAgreement.NewExceptionAgreement($"exception-agreement-{index}", "sample"));
        }
        var report = ExceptionAgreementReport.BuildExceptionAgreementReport("test", items, clock);
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
        ExceptionAgreementReportCountsEachRecordOnce();
    }
}
