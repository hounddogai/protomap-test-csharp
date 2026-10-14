namespace Acme.Workforce.Engagement.Recognition.Review;

using Acme.Workforce.Engagement.TownHall.OverrideItem;

/// <summary>Functions of recognition draft from override.</summary>
public static class RecognitionDraftFromOverride
{
    public static Dictionary<string, double> SummarizeTownHallOverridesForDraft(List<TownHallOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<RecognitionDraft> DeriveRecognitionDraftsFromOverride(List<TownHallOverride> items)
    {
        var outValue = new List<RecognitionDraft>();
        foreach (var item in items)
        {
            var derived = RecognitionDraft.NewRecognitionDraft(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveRecognitionDraft(RecognitionDraft item)
    {
        return TownHallProposalPolicy.NormalizeTownHallProposalCode(item.Code);
    }
}
