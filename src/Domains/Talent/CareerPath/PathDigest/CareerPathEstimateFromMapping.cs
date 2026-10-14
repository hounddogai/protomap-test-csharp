namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using Acme.Workforce.Talent.Mobility.Mapping;

/// <summary>Functions of career path estimate from mapping.</summary>
public static class CareerPathEstimateFromMapping
{
    public static Dictionary<string, double> SummarizeMobilityMappingsForEstimate(List<MobilityMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<CareerPathEstimate> DeriveCareerPathEstimatesFromMapping(List<MobilityMapping> items)
    {
        var outValue = new List<CareerPathEstimate>();
        foreach (var item in items)
        {
            var derived = CareerPathEstimate.NewCareerPathEstimate(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeCareerPathEstimate(CareerPathEstimate item)
    {
        return MobilityMappingPolicy.MobilityMappingBucket(item.EffectiveExposure());
    }
}
