namespace Acme.Workforce.Engagement.Tests.Recognition.Agreement;

using Acme.Workforce.Engagement.Recognition.Agreement;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class RecognitionAgreementReportChecks
{
    public static void RecognitionAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708042156);
        var items = new List<RecognitionAgreement>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RecognitionAgreement.NewRecognitionAgreement($"recognition-agreement-{index}", "sample"));
        }
        var report = RecognitionAgreementReport.BuildRecognitionAgreementReport("test", items, clock);
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
        RecognitionAgreementReportCountsEachRecordOnce();
    }
}
