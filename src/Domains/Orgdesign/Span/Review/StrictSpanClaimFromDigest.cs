namespace Acme.Workforce.Orgdesign.Span.Review;

using Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

/// <summary>Functions of strict span claim from digest.</summary>
public static class StrictSpanClaimFromDigest
{
    public static Dictionary<string, double> SummarizeCostModelDigestsForClaim(List<CostModelDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<StrictSpanClaim> DeriveStrictSpanClaimsFromDigest(List<CostModelDigest> items)
    {
        var outValue = new List<StrictSpanClaim>();
        foreach (var item in items)
        {
            var derived = StrictSpanClaim.NewStrictSpanClaim(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeStrictSpanClaim(StrictSpanClaim item)
    {
        return CostModelDigestPolicy.NormalizeCostModelDigestCode(item.Code);
    }
}
