namespace Acme.Workforce.Orgdesign.Headcount.Entry;

using Acme.Workforce.Orgdesign.CostModel.Manifest;

/// <summary>Functions of default headcount entry from manifest.</summary>
public static class DefaultHeadcountEntryFromManifest
{
    public static Dictionary<string, double> SummarizeDefaultCostModelManifestsForEntry(List<DefaultCostModelManifest> items)
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

    public static List<DefaultHeadcountEntry> DeriveDefaultHeadcountEntriesFromManifest(List<DefaultCostModelManifest> items)
    {
        var outValue = new List<DefaultHeadcountEntry>();
        foreach (var item in items)
        {
            var derived = DefaultHeadcountEntry.NewDefaultHeadcountEntry(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeDefaultHeadcountEntry(DefaultHeadcountEntry item)
    {
        return DefaultCostModelManifestPolicy.DefaultCostModelManifestWeight(7, true);
    }
}
