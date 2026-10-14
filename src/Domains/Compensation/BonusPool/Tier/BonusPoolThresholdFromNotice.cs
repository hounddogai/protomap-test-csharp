namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Compensation.BonusPool.Request;

/// <summary>Functions of bonus pool threshold from notice.</summary>
public static class BonusPoolThresholdFromNotice
{
    public static Dictionary<string, double> SummarizePrimaryBonusPoolNoticesForThreshold(List<PrimaryBonusPoolNotice> items)
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

    public static List<BonusPoolThreshold> DeriveBonusPoolThresholdsFromNotice(List<PrimaryBonusPoolNotice> items)
    {
        var outValue = new List<BonusPoolThreshold>();
        foreach (var item in items)
        {
            var derived = BonusPoolThreshold.NewBonusPoolThreshold(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveBonusPoolThreshold(BonusPoolThreshold item)
    {
        return PrimaryBonusPoolNoticePolicy.PrimaryBonusPoolNoticeBucket(item.ProjectedLoad());
    }
}
