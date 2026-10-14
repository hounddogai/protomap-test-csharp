namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyRecognitionBudgetReportChecks
{
    public static void LegacyRecognitionBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703478748);
        var items = new List<LegacyRecognitionBudget>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LegacyRecognitionBudget.NewLegacyRecognitionBudget($"legacy-recognition-budget-{index}", "sample"));
        }
        var report = LegacyRecognitionBudgetReport.BuildLegacyRecognitionBudgetReport("test", items, clock);
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
        LegacyRecognitionBudgetReportCountsEachRecordOnce();
    }
}
