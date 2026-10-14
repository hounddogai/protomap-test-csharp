namespace Acme.Workforce.Assets.Tests.AssetReturn.Rule;

using Acme.Workforce.Assets.AssetReturn.Rule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class StrictAssetReturnManifestReportChecks
{
    public static void StrictAssetReturnManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707140537);
        var items = new List<StrictAssetReturnManifest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StrictAssetReturnManifest.NewStrictAssetReturnManifest($"strict-asset-return-manifest-{index}", "sample"));
        }
        var report = StrictAssetReturnManifestReport.BuildStrictAssetReturnManifestReport("test", items, clock);
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
        StrictAssetReturnManifestReportCountsEachRecordOnce();
    }
}
