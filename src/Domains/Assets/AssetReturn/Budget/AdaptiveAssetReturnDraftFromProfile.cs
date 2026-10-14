namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Assets.Assignment.Profile;

/// <summary>Functions of adaptive asset return draft from profile.</summary>
public static class AdaptiveAssetReturnDraftFromProfile
{
    public static Dictionary<string, double> SummarizeAssignmentProfilesForDraft(List<AssignmentProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<AdaptiveAssetReturnDraft> DeriveAdaptiveAssetReturnDraftsFromProfile(List<AssignmentProfile> items)
    {
        var outValue = new List<AdaptiveAssetReturnDraft>();
        foreach (var item in items)
        {
            var derived = AdaptiveAssetReturnDraft.NewAdaptiveAssetReturnDraft(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateAdaptiveAssetReturnDraft(AdaptiveAssetReturnDraft item)
    {
        return RegionalAssignmentRulePolicy.ClassifyRegionalAssignmentRule(item.WeightedLoad());
    }
}
