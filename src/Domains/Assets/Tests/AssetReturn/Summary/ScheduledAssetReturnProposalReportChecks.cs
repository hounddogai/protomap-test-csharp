namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ScheduledAssetReturnProposalReportChecks
{
    public static void ScheduledAssetReturnProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708400007);
        var items = new List<ScheduledAssetReturnProposal>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScheduledAssetReturnProposal.NewScheduledAssetReturnProposal($"scheduled-asset-return-proposal-{index}", "sample"));
        }
        var report = ScheduledAssetReturnProposalReport.BuildScheduledAssetReturnProposalReport("test", items, clock);
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
        ScheduledAssetReturnProposalReportCountsEachRecordOnce();
    }
}
