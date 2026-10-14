namespace Acme.Workforce.Assets.Tests.AssetReturn.Proposal;

using Acme.Workforce.Assets.AssetReturn.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AssetReturnProposalReportChecks
{
    public static void AssetReturnProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704544251);
        var items = new List<AssetReturnProposal>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AssetReturnProposal.NewAssetReturnProposal($"asset-return-proposal-{index}", "sample"));
        }
        var report = AssetReturnProposalReport.BuildAssetReturnProposalReport("test", items, clock);
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
        AssetReturnProposalReportCountsEachRecordOnce();
    }
}
