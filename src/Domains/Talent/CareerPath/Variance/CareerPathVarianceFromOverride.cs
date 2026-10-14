namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Talent.CareerPath.Plan;

/// <summary>Functions of career path variance from override.</summary>
public static class CareerPathVarianceFromOverride
{
    public static Dictionary<string, double> SummarizeWeightedCareerPathOverridesForVariance(List<WeightedCareerPathOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<CareerPathVariance> DeriveCareerPathVariancesFromOverride(List<WeightedCareerPathOverride> items)
    {
        var outValue = new List<CareerPathVariance>();
        foreach (var item in items)
        {
            var derived = CareerPathVariance.NewCareerPathVariance(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectCareerPathVariance(CareerPathVariance item)
    {
        return WeightedCareerPathOverridePolicy.NormalizeWeightedCareerPathOverrideCode(item.Code);
    }
}
