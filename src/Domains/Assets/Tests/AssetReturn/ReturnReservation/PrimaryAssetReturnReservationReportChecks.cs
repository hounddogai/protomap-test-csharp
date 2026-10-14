namespace Acme.Workforce.Assets.Tests.AssetReturn.ReturnReservation;

using Acme.Workforce.Assets.AssetReturn.ReturnReservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of return reservation code, run by the project's runner.</summary>
public static class PrimaryAssetReturnReservationReportChecks
{
    public static void PrimaryAssetReturnReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705785326);
        var items = new List<PrimaryAssetReturnReservation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PrimaryAssetReturnReservation.NewPrimaryAssetReturnReservation($"primary-asset-return-reservation-{index}", "sample"));
        }
        var report = PrimaryAssetReturnReservationReport.BuildPrimaryAssetReturnReservationReport("test", items, clock);
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
        PrimaryAssetReturnReservationReportCountsEachRecordOnce();
    }
}
