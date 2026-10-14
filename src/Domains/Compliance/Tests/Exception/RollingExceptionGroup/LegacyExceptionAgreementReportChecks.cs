namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class LegacyExceptionAgreementReportChecks
{
    public static void LegacyExceptionAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703023041);
        var items = new List<LegacyExceptionAgreement>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyExceptionAgreement.NewLegacyExceptionAgreement($"legacy-exception-agreement-{index}", "sample"));
        }
        var report = LegacyExceptionAgreementReport.BuildLegacyExceptionAgreementReport("test", items, clock);
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
        LegacyExceptionAgreementReportCountsEachRecordOnce();
    }
}
