namespace Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Functions of cost model quota from score.</summary>
public static class CostModelQuotaFromScore
{
    public static Dictionary<string, double> SummarizePositionScoresForQuota(List<PositionScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<CostModelQuota> DeriveCostModelQuotasFromScore(List<PositionScore> items)
    {
        var outValue = new List<CostModelQuota>();
        foreach (var item in items)
        {
            var derived = CostModelQuota.NewCostModelQuota(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeCostModelQuota(CostModelQuota item)
    {
        return PositionSummaryPolicy.ClassifyPositionSummary(item.NetValue());
    }
}
