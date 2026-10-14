namespace Acme.Workforce.Talent.CareerPath.Mapping;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Functions of career path mapping from batch.</summary>
public static class CareerPathMappingFromBatch
{
    public static Dictionary<string, double> SummarizeSuccessionPlanBatchesForMapping(List<SuccessionPlanBatch> items)
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

    public static List<CareerPathMapping> DeriveCareerPathMappingsFromBatch(List<SuccessionPlanBatch> items)
    {
        var outValue = new List<CareerPathMapping>();
        foreach (var item in items)
        {
            var derived = CareerPathMapping.NewCareerPathMapping(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveCareerPathMapping(CareerPathMapping item)
    {
        return FallbackSuccessionPlanEntryPolicy.FallbackSuccessionPlanEntryBucket(item.ProjectedTotal());
    }
}
