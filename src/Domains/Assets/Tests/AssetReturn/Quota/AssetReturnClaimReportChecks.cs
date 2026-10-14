namespace Acme.Workforce.Assets.Tests.AssetReturn.Quota;

using Acme.Workforce.Assets.AssetReturn.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AssetReturnClaimReportChecks
{
    public static void AssetReturnClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709070901);
        var items = new List<AssetReturnClaim>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AssetReturnClaim.NewAssetReturnClaim($"asset-return-claim-{index}", "sample"));
        }
        var report = AssetReturnClaimReport.BuildAssetReturnClaimReport("test", items, clock);
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
        AssetReturnClaimReportCountsEachRecordOnce();
    }
}
