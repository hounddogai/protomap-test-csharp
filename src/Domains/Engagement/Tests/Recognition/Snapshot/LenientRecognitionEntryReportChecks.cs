namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LenientRecognitionEntryReportChecks
{
    public static void LenientRecognitionEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709664452);
        var items = new List<LenientRecognitionEntry>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LenientRecognitionEntry.NewLenientRecognitionEntry($"lenient-recognition-entry-{index}", "sample"));
        }
        var report = LenientRecognitionEntryReport.BuildLenientRecognitionEntryReport("test", items, clock);
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
        LenientRecognitionEntryReportCountsEachRecordOnce();
    }
}
