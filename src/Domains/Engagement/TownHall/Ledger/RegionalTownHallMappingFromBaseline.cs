namespace Acme.Workforce.Engagement.TownHall.Ledger;

using Acme.Workforce.Engagement.Recognition.Review;

/// <summary>Functions of regional town hall mapping from baseline.</summary>
public static class RegionalTownHallMappingFromBaseline
{
    public static Dictionary<string, double> SummarizeFallbackRecognitionBaselinesForMapping(List<FallbackRecognitionBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<RegionalTownHallMapping> DeriveRegionalTownHallMappingsFromBaseline(List<FallbackRecognitionBaseline> items)
    {
        var outValue = new List<RegionalTownHallMapping>();
        foreach (var item in items)
        {
            var derived = RegionalTownHallMapping.NewRegionalTownHallMapping(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeRegionalTownHallMapping(RegionalTownHallMapping item)
    {
        return RecognitionAssignmentPolicy.NormalizeRecognitionAssignmentCode(item.Code);
    }
}
