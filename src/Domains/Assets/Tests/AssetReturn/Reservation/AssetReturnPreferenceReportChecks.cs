namespace Acme.Workforce.Assets.Tests.AssetReturn.Reservation;

using Acme.Workforce.Assets.AssetReturn.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssetReturnPreferenceReportChecks
{
    public static void AssetReturnPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703491435);
        var items = new List<AssetReturnPreference>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AssetReturnPreference.NewAssetReturnPreference($"asset-return-preference-{index}", "sample"));
        }
        var report = AssetReturnPreferenceReport.BuildAssetReturnPreferenceReport("test", items, clock);
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
        AssetReturnPreferenceReportCountsEachRecordOnce();
    }
}
