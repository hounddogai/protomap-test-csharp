namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Functions of mobility window from draft.</summary>
public static class MobilityWindowFromDraft
{
    public static Dictionary<string, double> SummarizeCareerPathDraftsForWindow(List<CareerPathDraft> items)
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

    public static List<MobilityWindow> DeriveMobilityWindowsFromDraft(List<CareerPathDraft> items)
    {
        var outValue = new List<MobilityWindow>();
        foreach (var item in items)
        {
            var derived = MobilityWindow.NewMobilityWindow(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreMobilityWindow(MobilityWindow item)
    {
        return SeasonalCareerPathDigestPolicy.ClassifySeasonalCareerPathDigest(item.WeightedLoad());
    }
}
