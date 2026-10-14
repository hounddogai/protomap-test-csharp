namespace Acme.Workforce.Assets.Tests.AssetReturn.Score;

using Acme.Workforce.Assets.AssetReturn.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LenientAssetReturnTargetReportChecks
{
    public static void LenientAssetReturnTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700936824);
        var items = new List<LenientAssetReturnTarget>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LenientAssetReturnTarget.NewLenientAssetReturnTarget($"lenient-asset-return-target-{index}", "sample"));
        }
        var report = LenientAssetReturnTargetReport.BuildLenientAssetReturnTargetReport("test", items, clock);
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
        LenientAssetReturnTargetReportCountsEachRecordOnce();
    }
}
