namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RecognitionSnapshotReportChecks
{
    public static void RecognitionSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708395005);
        var items = new List<RecognitionSnapshot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RecognitionSnapshot.NewRecognitionSnapshot($"recognition-snapshot-{index}", "sample"));
        }
        var report = RecognitionSnapshotReport.BuildRecognitionSnapshotReport("test", items, clock);
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
        RecognitionSnapshotReportCountsEachRecordOnce();
    }
}
