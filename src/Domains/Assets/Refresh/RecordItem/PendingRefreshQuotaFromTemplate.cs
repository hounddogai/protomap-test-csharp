namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.Template;

/// <summary>Functions of pending refresh quota from template.</summary>
public static class PendingRefreshQuotaFromTemplate
{
    public static Dictionary<string, double> SummarizeLicenseSeatTemplatesForQuota(List<LicenseSeatTemplate> items)
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

    public static List<PendingRefreshQuota> DerivePendingRefreshQuotasFromTemplate(List<LicenseSeatTemplate> items)
    {
        var outValue = new List<PendingRefreshQuota>();
        foreach (var item in items)
        {
            var derived = PendingRefreshQuota.NewPendingRefreshQuota(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DerivePendingRefreshQuota(PendingRefreshQuota item)
    {
        return PendingLicenseSeatCheckpointPolicy.NormalizePendingLicenseSeatCheckpointCode(item.Code);
    }
}
