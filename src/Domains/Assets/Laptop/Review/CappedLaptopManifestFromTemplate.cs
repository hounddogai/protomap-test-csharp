namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Assets.Refresh.Checkpoint;

/// <summary>Functions of capped laptop manifest from template.</summary>
public static class CappedLaptopManifestFromTemplate
{
    public static Dictionary<string, double> SummarizeRefreshTemplatesForManifest(List<RefreshTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<CappedLaptopManifest> DeriveCappedLaptopManifestsFromTemplate(List<RefreshTemplate> items)
    {
        var outValue = new List<CappedLaptopManifest>();
        foreach (var item in items)
        {
            var derived = CappedLaptopManifest.NewCappedLaptopManifest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchCappedLaptopManifest(CappedLaptopManifest item)
    {
        return RefreshTemplatePolicy.NormalizeRefreshTemplateCode(item.Code);
    }
}
