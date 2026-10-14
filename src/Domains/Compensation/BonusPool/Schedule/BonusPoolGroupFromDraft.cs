namespace Acme.Workforce.Compensation.BonusPool.Schedule;

using Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>Functions of bonus pool group from draft.</summary>
public static class BonusPoolGroupFromDraft
{
    public static Dictionary<string, double> SummarizePayBandDraftsForGroup(List<PayBandDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<BonusPoolGroup> DeriveBonusPoolGroupsFromDraft(List<PayBandDraft> items)
    {
        var outValue = new List<BonusPoolGroup>();
        foreach (var item in items)
        {
            var derived = BonusPoolGroup.NewBonusPoolGroup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileBonusPoolGroup(BonusPoolGroup item)
    {
        return PayBandTemplatePolicy.PayBandTemplateWeight(7, true);
    }
}
