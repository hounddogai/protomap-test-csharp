namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Functions of town hall notice from policy.</summary>
public static class TownHallNoticeFromPolicy
{
    public static Dictionary<string, double> SummarizeFeedbackPoliciesForNotice(List<FeedbackPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<TownHallNotice> DeriveTownHallNoticesFromPolicy(List<FeedbackPolicy> items)
    {
        var outValue = new List<TownHallNotice>();
        foreach (var item in items)
        {
            var derived = TownHallNotice.NewTownHallNotice(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeTownHallNotice(TownHallNotice item)
    {
        return FeedbackQuotaPolicy.NormalizeFeedbackQuotaCode(item.Code);
    }
}
