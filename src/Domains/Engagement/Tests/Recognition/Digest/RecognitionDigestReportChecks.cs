namespace Acme.Workforce.Engagement.Tests.Recognition.Digest;

using Acme.Workforce.Engagement.Recognition.Digest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class RecognitionDigestReportChecks
{
    public static void RecognitionDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700705774);
        var items = new List<RecognitionDigest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RecognitionDigest.NewRecognitionDigest($"recognition-digest-{index}", "sample"));
        }
        var report = RecognitionDigestReport.BuildRecognitionDigestReport("test", items, clock);
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
        RecognitionDigestReportCountsEachRecordOnce();
    }
}
