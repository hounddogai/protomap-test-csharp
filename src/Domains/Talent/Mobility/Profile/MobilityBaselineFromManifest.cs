namespace Acme.Workforce.Talent.Mobility.Profile;

using Acme.Workforce.Talent.CareerPath.Agreement;

/// <summary>Functions of mobility baseline from manifest.</summary>
public static class MobilityBaselineFromManifest
{
    public static Dictionary<string, double> SummarizeCareerPathManifestsForBaseline(List<CareerPathManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<MobilityBaseline> DeriveMobilityBaselinesFromManifest(List<CareerPathManifest> items)
    {
        var outValue = new List<MobilityBaseline>();
        foreach (var item in items)
        {
            var derived = MobilityBaseline.NewMobilityBaseline(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanMobilityBaseline(MobilityBaseline item)
    {
        return CareerPathManifestPolicy.NormalizeCareerPathManifestCode(item.Code);
    }
}
