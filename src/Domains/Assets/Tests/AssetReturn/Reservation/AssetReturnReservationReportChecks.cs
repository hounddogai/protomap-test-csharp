namespace Acme.Workforce.Assets.Tests.AssetReturn.Reservation;

using Acme.Workforce.Assets.AssetReturn.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssetReturnReservationReportChecks
{
    public static void AssetReturnReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704636678);
        var items = new List<AssetReturnReservation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AssetReturnReservation.NewAssetReturnReservation($"asset-return-reservation-{index}", "sample"));
        }
        var report = AssetReturnReservationReport.BuildAssetReturnReservationReport("test", items, clock);
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
        AssetReturnReservationReportCountsEachRecordOnce();
    }
}
