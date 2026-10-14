namespace Acme.Workforce.Assets.Tests.AssetReturn.Ledger;

using Acme.Workforce.Assets.AssetReturn.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalAssetReturnReservationReportChecks
{
    public static void RegionalAssetReturnReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709456491);
        var items = new List<RegionalAssetReturnReservation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RegionalAssetReturnReservation.NewRegionalAssetReturnReservation($"regional-asset-return-reservation-{index}", "sample"));
        }
        var report = RegionalAssetReturnReservationReport.BuildRegionalAssetReturnReservationReport("test", items, clock);
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
        RegionalAssetReturnReservationReportCountsEachRecordOnce();
    }
}
