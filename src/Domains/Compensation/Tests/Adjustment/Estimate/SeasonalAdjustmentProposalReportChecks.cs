namespace Acme.Workforce.Compensation.Tests.Adjustment.Estimate;

using Acme.Workforce.Compensation.Adjustment.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SeasonalAdjustmentProposalReportChecks
{
    public static void SeasonalAdjustmentProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709651815);
        var items = new List<SeasonalAdjustmentProposal>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SeasonalAdjustmentProposal.NewSeasonalAdjustmentProposal($"seasonal-adjustment-proposal-{index}", "sample"));
        }
        var report = SeasonalAdjustmentProposalReport.BuildSeasonalAdjustmentProposalReport("test", items, clock);
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
        SeasonalAdjustmentProposalReportCountsEachRecordOnce();
    }
}
