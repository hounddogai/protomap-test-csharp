namespace Acme.Workforce.Orgdesign.Position.Manifest;

using Acme.Workforce.Orgdesign.Position.Entry;

/// <summary>Functions of position manifest from entry.</summary>
public static class PositionManifestFromEntry
{
    public static Dictionary<string, double> SummarizePositionEntriesForManifest(List<PositionEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<PositionManifest> DerivePositionManifestsFromEntry(List<PositionEntry> items)
    {
        var outValue = new List<PositionManifest>();
        foreach (var item in items)
        {
            var derived = PositionManifest.NewPositionManifest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettlePositionManifest(PositionManifest item)
    {
        return PositionGroupPolicy.NormalizePositionGroupCode(item.Code);
    }
}
