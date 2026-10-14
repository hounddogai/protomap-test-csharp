namespace Acme.Workforce.Orgdesign.Headcount.Target;

using Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>Functions of headcount target from variance.</summary>
public static class HeadcountTargetFromVariance
{
    public static Dictionary<string, double> SummarizeGlobalPositionVariancesForTarget(List<GlobalPositionVariance> items)
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

    public static List<HeadcountTarget> DeriveHeadcountTargetsFromVariance(List<GlobalPositionVariance> items)
    {
        var outValue = new List<HeadcountTarget>();
        foreach (var item in items)
        {
            var derived = HeadcountTarget.NewHeadcountTarget(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveHeadcountTarget(HeadcountTarget item)
    {
        return PositionQuotaPolicy.PositionQuotaBucket(item.ProjectedExposure());
    }
}
