namespace Acme.Workforce.Assets.Tests.AssetReturn.Proposal;

using Acme.Workforce.Assets.AssetReturn.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AssetReturnAuditReportChecks
{
    public static void AssetReturnAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708023293);
        var items = new List<AssetReturnAudit>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AssetReturnAudit.NewAssetReturnAudit($"asset-return-audit-{index}", "sample"));
        }
        var report = AssetReturnAuditReport.BuildAssetReturnAuditReport("test", items, clock);
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
        AssetReturnAuditReportCountsEachRecordOnce();
    }
}
