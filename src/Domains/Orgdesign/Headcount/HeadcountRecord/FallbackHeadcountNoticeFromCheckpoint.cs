namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Orgdesign.CostModel.Summary;

/// <summary>Functions of fallback headcount notice from checkpoint.</summary>
public static class FallbackHeadcountNoticeFromCheckpoint
{
    public static Dictionary<string, double> SummarizeStrictCostModelCheckpointsForNotice(List<StrictCostModelCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<FallbackHeadcountNotice> DeriveFallbackHeadcountNoticesFromCheckpoint(List<StrictCostModelCheckpoint> items)
    {
        var outValue = new List<FallbackHeadcountNotice>();
        foreach (var item in items)
        {
            var derived = FallbackHeadcountNotice.NewFallbackHeadcountNotice(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeFallbackHeadcountNotice(FallbackHeadcountNotice item)
    {
        return CostModelSummaryPolicy.NormalizeCostModelSummaryCode(item.Code);
    }
}
