namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Talent.Pool.Assignment;

/// <summary>Functions of nomination request from assignment.</summary>
public static class NominationRequestFromAssignment
{
    public static Dictionary<string, double> SummarizeAdaptivePoolAssignmentsForRequest(List<AdaptivePoolAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<NominationRequest> DeriveNominationRequestsFromAssignment(List<AdaptivePoolAssignment> items)
    {
        var outValue = new List<NominationRequest>();
        foreach (var item in items)
        {
            var derived = NominationRequest.NewNominationRequest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleNominationRequest(NominationRequest item)
    {
        return PoolAllocationPolicy.PoolAllocationWeight(2, true);
    }
}
