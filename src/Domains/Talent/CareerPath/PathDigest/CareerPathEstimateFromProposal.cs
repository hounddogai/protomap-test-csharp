namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>Functions of career path estimate from proposal.</summary>
public static class CareerPathEstimateFromProposal
{
    public static Dictionary<string, double> SummarizeCareerPathProposalsForEstimate(List<CareerPathProposal> items)
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

    public static List<CareerPathEstimate> DeriveCareerPathEstimatesFromProposal(List<CareerPathProposal> items)
    {
        var outValue = new List<CareerPathEstimate>();
        foreach (var item in items)
        {
            var derived = CareerPathEstimate.NewCareerPathEstimate(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignCareerPathEstimate(CareerPathEstimate item)
    {
        return ManualCareerPathEntryPolicy.ManualCareerPathEntryBucket(item.EffectiveExposure());
    }
}
