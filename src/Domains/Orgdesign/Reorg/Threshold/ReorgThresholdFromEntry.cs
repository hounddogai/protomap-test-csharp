namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using Acme.Workforce.Orgdesign.Headcount.Entry;

/// <summary>Functions of reorg threshold from entry.</summary>
public static class ReorgThresholdFromEntry
{
    public static Dictionary<string, double> SummarizeHeadcountEntriesForThreshold(List<HeadcountEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<ReorgThreshold> DeriveReorgThresholdsFromEntry(List<HeadcountEntry> items)
    {
        var outValue = new List<ReorgThreshold>();
        foreach (var item in items)
        {
            var derived = ReorgThreshold.NewReorgThreshold(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizeReorgThreshold(ReorgThreshold item)
    {
        return ManualHeadcountRulePolicy.ManualHeadcountRuleWeight(5, true);
    }
}
