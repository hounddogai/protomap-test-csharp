namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Compensation.PayBand.Summary;

/// <summary>Functions of bonus pool policy from estimate.</summary>
public static class BonusPoolPolicyFromEstimate
{
    public static Dictionary<string, double> SummarizeDefaultPayBandEstimatesForPolicy(List<DefaultPayBandEstimate> items)
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

    public static List<BonusPoolPolicy> DeriveBonusPoolPoliciesFromEstimate(List<DefaultPayBandEstimate> items)
    {
        var outValue = new List<BonusPoolPolicy>();
        foreach (var item in items)
        {
            var derived = BonusPoolPolicy.NewBonusPoolPolicy(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveBonusPoolPolicy(BonusPoolPolicy item)
    {
        return DefaultPayBandEstimatePolicy.NormalizeDefaultPayBandEstimateCode(item.Code);
    }
}
