namespace Acme.Workforce.Payrollops.PayRun.Variance;

using Acme.Workforce.Payrollops.RetroPay.Claim;

/// <summary>Functions of pay run digest from claim.</summary>
public static class PayRunDigestFromClaim
{
    public static Dictionary<string, double> SummarizeWeightedRetroPayClaimsForDigest(List<WeightedRetroPayClaim> items)
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

    public static List<PayRunDigest> DerivePayRunDigestsFromClaim(List<WeightedRetroPayClaim> items)
    {
        var outValue = new List<PayRunDigest>();
        foreach (var item in items)
        {
            var derived = PayRunDigest.NewPayRunDigest(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitPayRunDigest(PayRunDigest item)
    {
        return WeightedRetroPayClaimPolicy.NormalizeWeightedRetroPayClaimCode(item.Code);
    }
}
