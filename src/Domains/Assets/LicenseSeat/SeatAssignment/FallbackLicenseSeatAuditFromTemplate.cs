namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>Functions of fallback license seat audit from template.</summary>
public static class FallbackLicenseSeatAuditFromTemplate
{
    public static Dictionary<string, double> SummarizeAssetReturnTemplatesForAudit(List<AssetReturnTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<FallbackLicenseSeatAudit> DeriveFallbackLicenseSeatAuditsFromTemplate(List<AssetReturnTemplate> items)
    {
        var outValue = new List<FallbackLicenseSeatAudit>();
        foreach (var item in items)
        {
            var derived = FallbackLicenseSeatAudit.NewFallbackLicenseSeatAudit(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateFallbackLicenseSeatAudit(FallbackLicenseSeatAudit item)
    {
        return ManualAssetReturnBaselinePolicy.NormalizeManualAssetReturnBaselineCode(item.Code);
    }
}
