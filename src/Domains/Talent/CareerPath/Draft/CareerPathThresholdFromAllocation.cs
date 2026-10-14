namespace Acme.Workforce.Talent.CareerPath.Draft;

using Acme.Workforce.Talent.Mobility.Target;

/// <summary>Functions of career path threshold from allocation.</summary>
public static class CareerPathThresholdFromAllocation
{
    public static Dictionary<string, double> SummarizeMobilityAllocationsForThreshold(List<MobilityAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<CareerPathThreshold> DeriveCareerPathThresholdsFromAllocation(List<MobilityAllocation> items)
    {
        var outValue = new List<CareerPathThreshold>();
        foreach (var item in items)
        {
            var derived = CareerPathThreshold.NewCareerPathThreshold(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveCareerPathThreshold(CareerPathThreshold item)
    {
        return MobilityNoticePolicy.NormalizeMobilityNoticeCode(item.Code);
    }
}
