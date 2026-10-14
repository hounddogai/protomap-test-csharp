namespace Acme.Workforce.Workplace.Desk.Policy;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Functions of default desk policy from draft.</summary>
public static class DefaultDeskPolicyFromDraft
{
    public static Dictionary<string, double> SummarizeSeasonalAccessBadgeDraftsForPolicy(List<SeasonalAccessBadgeDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<DefaultDeskPolicy> DeriveDefaultDeskPoliciesFromDraft(List<SeasonalAccessBadgeDraft> items)
    {
        var outValue = new List<DefaultDeskPolicy>();
        foreach (var item in items)
        {
            var derived = DefaultDeskPolicy.NewDefaultDeskPolicy(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankDefaultDeskPolicy(DefaultDeskPolicy item)
    {
        return SeasonalAccessBadgeDraftPolicy.SeasonalAccessBadgeDraftBucket(item.MeasuredValue());
    }
}
