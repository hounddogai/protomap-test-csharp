namespace Acme.Workforce.Engagement.Tests.Recognition.Score;

using Acme.Workforce.Engagement.Recognition.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RecognitionPolicyReportChecks
{
    public static void RecognitionPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709945752);
        var items = new List<RecognitionPolicy>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RecognitionPolicy.NewRecognitionPolicy($"recognition-policy-{index}", "sample"));
        }
        var report = RecognitionPolicyReport.BuildRecognitionPolicyReport("test", items, clock);
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
        RecognitionPolicyReportCountsEachRecordOnce();
    }
}
