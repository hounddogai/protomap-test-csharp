namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

using Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>Functions of headcount batch from rule.</summary>
public static class HeadcountBatchFromRule
{
    public static Dictionary<string, double> SummarizeCostModelRulesForBatch(List<CostModelRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<HeadcountBatch> DeriveHeadcountBatchesFromRule(List<CostModelRule> items)
    {
        var outValue = new List<HeadcountBatch>();
        foreach (var item in items)
        {
            var derived = HeadcountBatch.NewHeadcountBatch(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeHeadcountBatch(HeadcountBatch item)
    {
        return CostModelScorePolicy.CostModelScoreWeight(4, true);
    }
}
