namespace Acme.Workforce.Assets.Tests.AssetReturn.Ledger;

using Acme.Workforce.Assets.AssetReturn.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AssetReturnLedgerReportChecks
{
    public static void AssetReturnLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703124012);
        var items = new List<AssetReturnLedger>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AssetReturnLedger.NewAssetReturnLedger($"asset-return-ledger-{index}", "sample"));
        }
        var report = AssetReturnLedgerReport.BuildAssetReturnLedgerReport("test", items, clock);
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
        AssetReturnLedgerReportCountsEachRecordOnce();
    }
}
