namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Functions of capped equity grant score from entry.</summary>
public static class CappedEquityGrantScoreFromEntry
{
    public static Dictionary<string, double> SummarizeBonusPoolEntriesForScore(List<BonusPoolEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<CappedEquityGrantScore> DeriveCappedEquityGrantScoresFromEntry(List<BonusPoolEntry> items)
    {
        var outValue = new List<CappedEquityGrantScore>();
        foreach (var item in items)
        {
            var derived = CappedEquityGrantScore.NewCappedEquityGrantScore(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitCappedEquityGrantScore(CappedEquityGrantScore item)
    {
        return BonusPoolEntryPolicy.NormalizeBonusPoolEntryCode(item.Code);
    }
}
