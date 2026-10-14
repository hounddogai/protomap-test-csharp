namespace Acme.Workforce.Talent.CareerPath.Allocation;

using Acme.Workforce.Talent.SkillProfile.Threshold;

/// <summary>Functions of career path allocation from threshold.</summary>
public static class CareerPathAllocationFromThreshold
{
    public static Dictionary<string, double> SummarizeSkillProfileThresholdsForAllocation(List<SkillProfileThreshold> items)
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

    public static List<CareerPathAllocation> DeriveCareerPathAllocationsFromThreshold(List<SkillProfileThreshold> items)
    {
        var outValue = new List<CareerPathAllocation>();
        foreach (var item in items)
        {
            var derived = CareerPathAllocation.NewCareerPathAllocation(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateCareerPathAllocation(CareerPathAllocation item)
    {
        return DefaultSkillProfilePolicyPolicy.ClassifyDefaultSkillProfilePolicy(item.NetVolume());
    }
}
