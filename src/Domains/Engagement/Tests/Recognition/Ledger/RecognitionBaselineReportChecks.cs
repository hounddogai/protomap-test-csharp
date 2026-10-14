namespace Acme.Workforce.Engagement.Tests.Recognition.Ledger;

using Acme.Workforce.Engagement.Recognition.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RecognitionBaselineReportChecks
{
    public static void RecognitionBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709221658);
        var items = new List<RecognitionBaseline>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RecognitionBaseline.NewRecognitionBaseline($"recognition-baseline-{index}", "sample"));
        }
        var report = RecognitionBaselineReport.BuildRecognitionBaselineReport("test", items, clock);
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
        RecognitionBaselineReportCountsEachRecordOnce();
    }
}
