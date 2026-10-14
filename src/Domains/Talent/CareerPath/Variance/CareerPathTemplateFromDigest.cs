namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Talent.SuccessionPlan.Request;

/// <summary>Functions of career path template from digest.</summary>
public static class CareerPathTemplateFromDigest
{
    public static Dictionary<string, double> SummarizeSuccessionPlanDigestsForTemplate(List<SuccessionPlanDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<CareerPathTemplate> DeriveCareerPathTemplatesFromDigest(List<SuccessionPlanDigest> items)
    {
        var outValue = new List<CareerPathTemplate>();
        foreach (var item in items)
        {
            var derived = CareerPathTemplate.NewCareerPathTemplate(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateCareerPathTemplate(CareerPathTemplate item)
    {
        return SuccessionPlanDigestPolicy.SuccessionPlanDigestBucket(item.NetTotal());
    }
}
