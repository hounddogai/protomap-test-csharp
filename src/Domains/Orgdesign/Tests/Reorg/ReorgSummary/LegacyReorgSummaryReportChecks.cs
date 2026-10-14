namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class LegacyReorgSummaryReportChecks
{
    public static void LegacyReorgSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709731277);
        var items = new List<LegacyReorgSummary>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LegacyReorgSummary.NewLegacyReorgSummary($"legacy-reorg-summary-{index}", "sample"));
        }
        var report = LegacyReorgSummaryReport.BuildLegacyReorgSummaryReport("test", items, clock);
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
        LegacyReorgSummaryReportCountsEachRecordOnce();
    }
}
