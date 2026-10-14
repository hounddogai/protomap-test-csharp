namespace Acme.Workforce.Assets.Tests.AssetReturn.Assignment;

using Acme.Workforce.Assets.AssetReturn.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class FallbackAssetReturnPreferenceReportChecks
{
    public static void FallbackAssetReturnPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704249357);
        var items = new List<FallbackAssetReturnPreference>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FallbackAssetReturnPreference.NewFallbackAssetReturnPreference($"fallback-asset-return-preference-{index}", "sample"));
        }
        var report = FallbackAssetReturnPreferenceReport.BuildFallbackAssetReturnPreferenceReport("test", items, clock);
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
        FallbackAssetReturnPreferenceReportCountsEachRecordOnce();
    }
}
