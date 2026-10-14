namespace Acme.Workforce.Absence.Accrual.Group;

using Acme.Workforce.Absence.Accrual.AccrualManifest;

/// <summary>Functions of weighted accrual group from review.</summary>
public static class WeightedAccrualGroupFromReview
{
    public static Dictionary<string, double> SummarizeAccrualReviewsForGroup(List<AccrualReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<WeightedAccrualGroup> DeriveWeightedAccrualGroupsFromReview(List<AccrualReview> items)
    {
        var outValue = new List<WeightedAccrualGroup>();
        foreach (var item in items)
        {
            var derived = WeightedAccrualGroup.NewWeightedAccrualGroup(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyWeightedAccrualGroup(WeightedAccrualGroup item)
    {
        return PrimaryAccrualManifestPolicy.NormalizePrimaryAccrualManifestCode(item.Code);
    }
}
