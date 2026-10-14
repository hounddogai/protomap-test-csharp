namespace Acme.Workforce.Orgdesign.Headcount.Target;

using Acme.Workforce.Orgdesign.CostModel.Snapshot;

/// <summary>Functions of headcount target from manifest.</summary>
public static class HeadcountTargetFromManifest
{
    public static Dictionary<string, double> SummarizeCostModelManifestsForTarget(List<CostModelManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<HeadcountTarget> DeriveHeadcountTargetsFromManifest(List<CostModelManifest> items)
    {
        var outValue = new List<HeadcountTarget>();
        foreach (var item in items)
        {
            var derived = HeadcountTarget.NewHeadcountTarget(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectHeadcountTarget(HeadcountTarget item)
    {
        return CostModelManifestPolicy.NormalizeCostModelManifestCode(item.Code);
    }
}
