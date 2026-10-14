namespace Acme.Workforce.Assets.Laptop.Draft;

using Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>Functions of laptop draft from checkpoint.</summary>
public static class LaptopDraftFromCheckpoint
{
    public static Dictionary<string, double> SummarizeAssetReturnCheckpointsForDraft(List<AssetReturnCheckpoint> items)
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

    public static List<LaptopDraft> DeriveLaptopDraftsFromCheckpoint(List<AssetReturnCheckpoint> items)
    {
        var outValue = new List<LaptopDraft>();
        foreach (var item in items)
        {
            var derived = LaptopDraft.NewLaptopDraft(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveLaptopDraft(LaptopDraft item)
    {
        return AssetReturnAllocationPolicy.ClassifyAssetReturnAllocation(item.EffectiveTotal());
    }
}
