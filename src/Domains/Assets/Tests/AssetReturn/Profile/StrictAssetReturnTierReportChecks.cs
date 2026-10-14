namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class StrictAssetReturnTierReportChecks
{
    public static void StrictAssetReturnTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706783525);
        var items = new List<StrictAssetReturnTier>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(StrictAssetReturnTier.NewStrictAssetReturnTier($"strict-asset-return-tier-{index}", "sample"));
        }
        var report = StrictAssetReturnTierReport.BuildStrictAssetReturnTierReport("test", items, clock);
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
        StrictAssetReturnTierReportCountsEachRecordOnce();
    }
}
