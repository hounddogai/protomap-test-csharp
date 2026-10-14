namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Engagement.TownHall.Claim;

/// <summary>Functions of recognition override from claim.</summary>
public static class RecognitionOverrideFromClaim
{
    public static Dictionary<string, double> SummarizeTownHallClaimsForOverride(List<TownHallClaim> items)
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

    public static List<RecognitionOverride> DeriveRecognitionOverridesFromClaim(List<TownHallClaim> items)
    {
        var outValue = new List<RecognitionOverride>();
        foreach (var item in items)
        {
            var derived = RecognitionOverride.NewRecognitionOverride(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchRecognitionOverride(RecognitionOverride item)
    {
        return TownHallClaimPolicy.TownHallClaimBucket(item.MeasuredValue());
    }
}
