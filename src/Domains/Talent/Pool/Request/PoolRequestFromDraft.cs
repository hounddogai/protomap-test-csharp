namespace Acme.Workforce.Talent.Pool.Request;

using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Functions of pool request from draft.</summary>
public static class PoolRequestFromDraft
{
    public static Dictionary<string, double> SummarizeMobilityDraftsForRequest(List<MobilityDraft> items)
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

    public static List<PoolRequest> DerivePoolRequestsFromDraft(List<MobilityDraft> items)
    {
        var outValue = new List<PoolRequest>();
        foreach (var item in items)
        {
            var derived = PoolRequest.NewPoolRequest(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocatePoolRequest(PoolRequest item)
    {
        return AdaptiveMobilityProposalPolicy.NormalizeAdaptiveMobilityProposalCode(item.Code);
    }
}
