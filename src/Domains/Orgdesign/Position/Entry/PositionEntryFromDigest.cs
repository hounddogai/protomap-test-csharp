namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

/// <summary>Functions of position entry from digest.</summary>
public static class PositionEntryFromDigest
{
    public static Dictionary<string, double> SummarizeCostModelDigestsForEntry(List<CostModelDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<PositionEntry> DerivePositionEntriesFromDigest(List<CostModelDigest> items)
    {
        var outValue = new List<PositionEntry>();
        foreach (var item in items)
        {
            var derived = PositionEntry.NewPositionEntry(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchPositionEntry(PositionEntry item)
    {
        return CostModelDigestPolicy.NormalizeCostModelDigestCode(item.Code);
    }
}
