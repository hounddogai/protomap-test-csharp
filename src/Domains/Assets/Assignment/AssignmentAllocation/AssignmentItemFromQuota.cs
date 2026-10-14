namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

using Acme.Workforce.Assets.Laptop.Quota;

/// <summary>Functions of assignment item from quota.</summary>
public static class AssignmentItemFromQuota
{
    public static Dictionary<string, double> SummarizeLaptopQuotasForItem(List<LaptopQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<AssignmentItem> DeriveAssignmentItemsFromQuota(List<LaptopQuota> items)
    {
        var outValue = new List<AssignmentItem>();
        foreach (var item in items)
        {
            var derived = AssignmentItem.NewAssignmentItem(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanAssignmentItem(AssignmentItem item)
    {
        return LaptopExceptionPolicy.ClassifyLaptopException(item.NetLoad());
    }
}
