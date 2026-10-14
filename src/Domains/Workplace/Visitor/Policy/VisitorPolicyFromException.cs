namespace Acme.Workforce.Workplace.Visitor.Policy;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Functions of visitor policy from exception.</summary>
public static class VisitorPolicyFromException
{
    public static Dictionary<string, double> SummarizeOccupancyExceptionsForPolicy(List<OccupancyException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<VisitorPolicy> DeriveVisitorPoliciesFromException(List<OccupancyException> items)
    {
        var outValue = new List<VisitorPolicy>();
        foreach (var item in items)
        {
            var derived = VisitorPolicy.NewVisitorPolicy(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveVisitorPolicy(VisitorPolicy item)
    {
        return OccupancyAllocationPolicy.OccupancyAllocationWeight(3, true);
    }
}
